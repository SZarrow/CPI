using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using CPI.Common;
using CPI.Common.Domain.FundOut.YeePay;
using CPI.Common.Exceptions;
using CPI.Common.Models;
using CPI.Config;
using CPI.IData.BaseRepositories;
using CPI.IService.FundOut;
using CPI.Providers;
using CPI.Utils;
using Lotus.Core;
using Lotus.Logging;
using Lotus.Net;

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
                    batchNo = DateTime.Now.ToString("yyyyMMddHHmmssffff"),
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
