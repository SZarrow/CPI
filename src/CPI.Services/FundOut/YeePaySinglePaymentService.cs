using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using CPI.Common;
using CPI.Common.Domain.FundOut.YeePay;
using CPI.Common.Exceptions;
using CPI.Common.Models;
using CPI.Config;
using CPI.IData.BaseRepositories;
using CPI.IService.FundOut;
using CPI.Providers;
using CPI.Utils;
using ATBase.Core;
using ATBase.Core.Collections;
using ATBase.Logging;

namespace CPI.Services.FundOut
{
    public class YeePaySinglePaymentService : IYeePaySinglePaymentService
    {
        private static readonly ILogger _logger = LogManager.GetLogger();
        private static readonly HttpClient _client = CreateHttpClient();
        private static readonly LockProvider _lockProvider = new LockProvider();

        private readonly IFundOutOrderRepository _fundOutOrderRepository = null;

        public XResult<YeePaySinglePayResponse> Pay(YeePaySinglePayRequest request)
        {
            if (request == null)
            {
                return new XResult<YeePaySinglePayResponse>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentNullException(nameof(request)));
            }

            if (!request.IsValid)
            {
                return new XResult<YeePaySinglePayResponse>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentException(request.ErrorMessage));
            }

            String service = $"{this.GetType().FullName}.Pay(...)";
            var requestHash = $"pay:{request.AppId}.{request.OutTradeNo}".GetHashCode();

            if (_lockProvider.Exists(requestHash))
            {
                return new XResult<YeePaySinglePayResponse>(null, ErrorCode.SUBMIT_REPEAT);
            }

            try
            {
                if (!_lockProvider.Lock(requestHash))
                {
                    return new XResult<YeePaySinglePayResponse>(null, ErrorCode.SUBMIT_REPEAT);
                }

                var existsOutTradeNo = _fundOutOrderRepository.Exists(x => x.OutTradeNo == request.OutTradeNo);
                if (existsOutTradeNo)
                {
                    return new XResult<YeePaySinglePayResponse>(null, ErrorCode.OUT_TRADE_NO_EXISTED);
                }

                var now = DateTime.Now;
                var newId = IDGenerator.GenerateID();

                var fundoutOrder = new FundOutOrder()
                {
                    Id = newId,
                    TradeNo = newId.ToString(),
                    AppId = request.AppId,
                    OutTradeNo = request.OutTradeNo,
                    Amount = request.Amount.ToDecimal(),
                    RealName = request.AccountName,
                    BankCardNo = request.BankCardNo,
                    Remark = request.Remark,
                    PayStatus = PayStatus.APPLY.ToString(),
                    CreateTime = DateTime.Now
                };

                _fundOutOrderRepository.Add(fundoutOrder);
                var saveResult = _fundOutOrderRepository.SaveChanges();
                if (!saveResult.Success)
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(_fundOutOrderRepository)}.SaveChanges()", "保存代付订单数据失败", saveResult.FirstException, fundoutOrder);
                    return new XResult<YeePaySinglePayResponse>(null, ErrorCode.DB_UPDATE_FAILED, new DbUpdateException("保存代付订单数据失败"));
                }

                String traceMethod = $"{nameof(YeePayFundOutUtil)}.Execute(...)";

                _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.OK.ToString(), service, traceMethod, LogPhase.BEGIN, "开始调用易宝代付支付接口", request);

                //记录请求开始时间
                fundoutOrder.ApplyTime = DateTime.Now;

                var execResult = YeePayFundOutUtil.Execute<RawYeePaySinglePayRequest, RawYeePaySinglePayResult>("/rest/v1.0/balance/transfer_send", new RawYeePaySinglePayRequest()
                {
                    orderId = request.OutTradeNo,
                    accountName = request.AccountName,
                    accountNumber = request.BankCardNo,
                    amount = request.Amount,
                    bankCode = request.BankCode,
                    batchNo = fundoutOrder.TradeNo,
                    customerNumber = GlobalConfig.YeePay_FundOut_MerchantNo,
                    groupNumber = GlobalConfig.YeePay_FundOut_MerchantNo,
                    feeType = request.FeeType
                });

                //记录请求结束时间
                fundoutOrder.EndTime = DateTime.Now;

                _logger.Trace(TraceType.BLL.ToString(), (execResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), service, traceMethod, LogPhase.ACTION, "结束调用易宝代付支付接口");

                if (!execResult.Success)
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, traceMethod, "代付失败", execResult.FirstException, execResult);
                    return new XResult<YeePaySinglePayResponse>(null, ErrorCode.DEPENDENT_API_CALL_FAILED, new RequestException(execResult.ErrorMessage));
                }

                var respResult = execResult.Value;
                if (respResult.errorCode != "BAC001")
                {
                    fundoutOrder.PayStatus = PayStatus.FAILURE.ToString();
                    UpdateFundOutOrder(fundoutOrder);
                    return new XResult<YeePaySinglePayResponse>(null, ErrorCode.FAILURE, new RemoteException(respResult.errorMsg.HasValue() ? respResult.errorMsg : "发送请求失败"));
                }

                fundoutOrder.PayStatus = PayStatus.PROCESSING.ToString();
                switch (respResult.transferStatusCode)
                {
                    case "0028":
                        fundoutOrder.PayStatus = PayStatus.FAILURE.ToString();
                        break;
                }

                UpdateFundOutOrder(fundoutOrder);

                var payResp = new YeePaySinglePayResponse()
                {
                    BatchNo = respResult.batchNo,
                    OutTradeNo = request.OutTradeNo,
                    Status = fundoutOrder.PayStatus,
                    Msg = GetPayStatusDescription(fundoutOrder.PayStatus)
                };

                return new XResult<YeePaySinglePayResponse>(payResp);
            }
            finally
            {
                _lockProvider.UnLock(requestHash);
            }
        }

        public XResult<YeePaySinglePayResultQueryResponse> QueryStatus(YeePaySinglePayResultQueryRequest request)
        {
            if (request == null)
            {
                return new XResult<YeePaySinglePayResultQueryResponse>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentNullException(nameof(request)));
            }

            if (!request.IsValid)
            {
                return new XResult<YeePaySinglePayResultQueryResponse>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentException(request.ErrorMessage));
            }

            String service = $"{this.GetType().FullName}.QueryStatus(...)";
            var requestHash = $"QueryStatus:{request.OutTradeNo}{request.PageIndex}{request.PageSize}".GetHashCode();

            if (_lockProvider.Exists(requestHash))
            {
                return new XResult<YeePaySinglePayResultQueryResponse>(null, ErrorCode.SUBMIT_REPEAT);
            }

            try
            {
                var q = _fundOutOrderRepository.QueryProvider;

                if (request.OutTradeNo.HasValue())
                {
                    if (request.OutTradeNo.IndexOf(",") > 0)
                    {
                        var outTradeNos = request.OutTradeNo.Split(new Char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        q = from t0 in q
                            where outTradeNos.Contains(t0.OutTradeNo)
                            select t0;
                    }
                    else
                    {
                        q = from t0 in q
                            where t0.OutTradeNo == request.OutTradeNo
                            select t0;
                    }
                }

                if (request.From.HasValue() && DateTime.TryParse(request.From, out DateTime fromDate))
                {
                    fromDate = fromDate.Date;
                    q = from t0 in q
                        where t0.CreateTime >= fromDate
                        select t0;
                }

                if (request.To.HasValue() && DateTime.TryParse(request.To, out DateTime toDate))
                {
                    toDate = toDate.Date.AddDays(1);
                    q = from t0 in q
                        where t0.CreateTime < toDate
                        select t0;
                }

                var pagedList = new PagedList<FundOutOrder>(q.OrderByDescending(x => x.CreateTime), request.PageIndex.ToInt32(), request.PageSize.ToInt32());
                if (pagedList.Exception != null)
                {
                    return new XResult<YeePaySinglePayResultQueryResponse>(null, ErrorCode.DB_QUERY_FAILED, pagedList.Exception);
                }

                if (String.Compare(request.QueryMode, "PULL", true) == 0)
                {
                    if (_lockProvider.Lock(requestHash))
                    {
                        var needPullOrders = pagedList.Where(x => x.PayStatus != PayStatus.SUCCESS.ToString() && x.PayStatus != PayStatus.FAILURE.ToString());
                        if (needPullOrders != null && needPullOrders.Count() > 0)
                        {
                            Boolean statusChanged = false;
                            foreach (var pullOrder in needPullOrders)
                            {
                                var pullResult = YeePayFundOutUtil.Execute<RawYeePaySinglePayResultQueryStatusRequest, RawYeePaySinglePayResultQueryStatusResult>("/rest/v1.0/balance/transfer_query", new RawYeePaySinglePayResultQueryStatusRequest()
                                {
                                    batchNo = pullOrder.TradeNo,
                                    customerNumber = GlobalConfig.YeePay_FundOut_MerchantNo,
                                    orderId = pullOrder.OutTradeNo,
                                    pageNo = request.PageIndex,
                                    pageSize = request.PageSize,
                                    product = String.Empty
                                });

                                if (pullResult.Success && pullResult.Value != null)
                                {
                                    var orderResult = pullResult.Value;
                                    if (orderResult.errorCode == "BAC000048")
                                    {
                                        pullOrder.PayStatus = PayStatus.FAILURE.ToString();
                                        pullOrder.UpdateTime = DateTime.Now;
                                        statusChanged = true;
                                    }
                                    else if (orderResult.errorCode == "BAC001" && orderResult.list != null && orderResult.list.Count() == 1)
                                    {
                                        var orderResultDetail = orderResult.list.FirstOrDefault();
                                        if (orderResultDetail.transferStatusCode == "0028"
                                            || orderResultDetail.transferStatusCode == "0027")
                                        {
                                            pullOrder.PayStatus = PayStatus.FAILURE.ToString();
                                            pullOrder.UpdateTime = DateTime.Now;
                                            statusChanged = true;
                                        }
                                        else if (orderResultDetail.transferStatusCode == "0026")
                                        {
                                            switch (orderResultDetail.bankTrxStatusCode)
                                            {
                                                case "S":
                                                    pullOrder.Fee = orderResultDetail.fee.ToDecimal();
                                                    pullOrder.FeeAction = orderResultDetail.feeType;
                                                    pullOrder.Remark = orderResultDetail.leaveWord;
                                                    pullOrder.PayStatus = PayStatus.SUCCESS.ToString();
                                                    pullOrder.UpdateTime = DateTime.Now;
                                                    statusChanged = true;
                                                    break;
                                                case "F":
                                                    pullOrder.PayStatus = PayStatus.FAILURE.ToString();
                                                    pullOrder.UpdateTime = DateTime.Now;
                                                    statusChanged = true;
                                                    break;
                                            }
                                        }
                                    }
                                }

                                if (statusChanged)
                                {
                                    _fundOutOrderRepository.Update(pullOrder);
                                    var updateResult = _fundOutOrderRepository.SaveChanges();
                                    if (!updateResult.Success)
                                    {
                                        _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(_fundOutOrderRepository)}.SaveChanges()", "保存代付订单数据失败", updateResult.FirstException, new Object[] { pullOrder, pullResult.Value });
                                    }
                                }
                            }
                        }
                    }
                }

                var pageInfo = pagedList.PageInfo;

                var resp = new YeePaySinglePayResultQueryResponse()
                {
                    PageIndex = pageInfo.PageIndex,
                    PageSize = pageInfo.PageSize,
                    TotalCount = pageInfo.TotalCount,
                    Orders = pagedList.Select(x => new YeePaySinglePayStatusResult()
                    {
                        OutTradeNo = x.OutTradeNo,
                        BankCardNo = x.BankCardNo,
                        Amount = x.Amount,
                        Fee = x.Fee,
                        CreateTime = x.CreateTime,
                        Status = x.PayStatus,
                        Msg = GetPayStatusDescription(x.PayStatus)
                    })
                };

                return new XResult<YeePaySinglePayResultQueryResponse>(resp);
            }
            finally
            {
                _lockProvider.UnLock(requestHash);
            }
        }

        private String GetPayStatusDescription(String payStatus)
        {
            switch (payStatus)
            {
                case nameof(PayStatus.APPLY):
                    return PayStatus.APPLY.GetDescription();
                case nameof(PayStatus.PROCESSING):
                    return PayStatus.PROCESSING.GetDescription();
                case nameof(PayStatus.FAILURE):
                    return PayStatus.FAILURE.GetDescription();
                case nameof(PayStatus.SUCCESS):
                    return PayStatus.SUCCESS.GetDescription();
            }

            return "未知状态";
        }

        private void UpdateFundOutOrder(FundOutOrder fundoutOrder)
        {
            fundoutOrder.UpdateTime = DateTime.Now;
            _fundOutOrderRepository.Update(fundoutOrder);
            var updateResult = _fundOutOrderRepository.SaveChanges();
            if (!updateResult.Success)
            {
                _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), $"{this.GetType().FullName}.Pay(...)", $"{nameof(_fundOutOrderRepository)}.SaveChanges()", "更新支付状态失败", updateResult.FirstException, fundoutOrder);
            }
        }

        private static HttpClient CreateHttpClient()
        {
            var factory = XDI.Resolve<IHttpClientFactory>();
            return factory.CreateClient();
        }
    }
}
