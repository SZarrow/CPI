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

                //_fundOutOrderRepository.Add(fundoutOrder);
                //var saveResult = _fundOutOrderRepository.SaveChanges();
                //if (!saveResult.Success)
                //{
                //    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(_fundOutOrderRepository)}.SaveChanges()", "保存代付订单数据失败", saveResult.FirstException, fundoutOrder);
                //    return new XResult<YeePaySinglePayResponse>(null, ErrorCode.DB_UPDATE_FAILED, new DbUpdateException("保存代付订单数据失败"));
                //}

                //记录请求开始时间
                fundoutOrder.ApplyTime = DateTime.Now;
                String traceMethod = $"{nameof(_client)}.PostForm(...)";

                //var dic = CommonUtil.ToDictionary(request, true);

                //var respMsgResult = _client.PostForm(ApiConfig.YeePay_FundOut_Pay_RequestUrl, dic);

                var respMsgResult = YeePayFundOutUtil.Execute<RawYeePaySinglePayRequest, RawYeePaySinglePayResult>("/rest/v1.0/balance/transfer_send", new RawYeePaySinglePayRequest()
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

                //_logger.Trace(TraceType.BLL.ToString(), (respMsgResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), service, traceMethod, LogPhase.ACTION, $"结束调用{ApiConfig.EPay95_FundOut_Pay_RequestUrl}");

                //if (!respMsgResult.Success)
                //{
                //    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, traceMethod, "代付失败", respMsgResult.FirstException, dic);
                //    return new XResult<YeePaySinglePayResponse>(null, ErrorCode.DEPENDENT_API_CALL_FAILED, new RequestException(respMsgResult.ErrorMessage));
                //}

                //String respContent = null;
                //try
                //{
                //    respContent = respMsgResult.Value.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                //    _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.OK.ToString(), service, traceMethod, LogPhase.END, $"调用{ApiConfig.EPay95_FundOut_Pay_RequestUrl}返回结果", respContent);
                //}
                //catch (Exception ex)
                //{
                //    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, traceMethod, "读取代付返回的消息内容出现异常", ex, dic);
                //    return new XResult<YeePaySinglePayResponse>(null, ErrorCode.RESPONSE_READ_FAILED, ex);
                //}

                //if (respContent.IsNullOrWhiteSpace())
                //{
                //    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, traceMethod, "易宝未返回任何数据", null, dic);
                //    return new XResult<YeePaySinglePayResponse>(null, ErrorCode.REMOTE_RETURN_NOTHING);
                //}

                //EPay95PayReturnResult respResult = JsonUtil.DeserializeObject<EPay95PayReturnResult>(respContent).Value;
                //if (respResult == null)
                //{
                //    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, "respResult", "无法将代付结果反序列化为EPay95PayReturnResult对象", null, respContent);
                //    return new XResult<YeePaySinglePayResponse>(null, ErrorCode.DESERIALIZE_FAILED);
                //}

                ////记录请求结束时间
                //fundoutOrder.EndTime = DateTime.Now;

                //switch (respResult.ResultCode)
                //{
                //    case "88":
                //    case "90":
                //    case "15":
                //        //修改支付状态为正在处理中
                //        fundoutOrder.PayStatus = PayStatus.PROCESSING.ToString();
                //        break;
                //    default:
                //        //修改支付状态为正在处理中
                //        fundoutOrder.PayStatus = PayStatus.FAILURE.ToString();
                //        break;
                //}

                //fundoutOrder.UpdateTime = DateTime.Now;
                //_fundOutOrderRepository.Update(fundoutOrder);
                //var updateResult = _fundOutOrderRepository.SaveChanges();
                //if (!updateResult.Success)
                //{
                //    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(_fundOutOrderRepository)}.SaveChanges()", "更新支付状态失败", updateResult.FirstException, fundoutOrder);
                //}

                //var payResp = new YeePaySinglePayResponse()
                //{
                //    Status = fundoutOrder.PayStatus,
                //    Msg = respResult.Message.HasValue() ? respResult.Message : PayStatus.PROCESSING.GetDescription()
                //};

                //if (respResult.LoanJsonList != null)
                //{
                //    payResp.Amount = respResult.LoanJsonList.Amount;
                //    payResp.BankCardNo = respResult.LoanJsonList.CardNumber;
                //    payResp.OutTradeNo = respResult.LoanJsonList.OrderNo;
                //}

                return new XResult<YeePaySinglePayResponse>(new YeePaySinglePayResponse());
            }
            finally
            {
                _lockProvider.UnLock(requestHash);
            }

            throw new NotImplementedException();
        }

        private static HttpClient CreateHttpClient()
        {
            var factory = XDI.Resolve<IHttpClientFactory>();
            return factory.CreateClient();
        }
    }
}
