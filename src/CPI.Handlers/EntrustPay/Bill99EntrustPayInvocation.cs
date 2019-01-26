using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPI.Common;
using CPI.Common.Domain.AgreePay;
using CPI.Common.Domain.EntrustPay;
using CPI.Common.Exceptions;
using CPI.Config;
using CPI.IService.AgreePay;
using CPI.IService.EntrustPay;
using CPI.Utils;
using Lotus.Core;
using Lotus.Logging;

namespace CPI.Handlers.EntrustPay
{
    internal class Bill99EntrustPayInvocation : IInvocation
    {
        private static readonly ILogger _logger = LogManager.GetLogger();

        private readonly GatewayCommonRequest _request;
        private readonly IEntrustPaymentService _entrustPayService;
        private readonly IAgreePayBankCardBindInfoService _bindInfoService;

        public Bill99EntrustPayInvocation(GatewayCommonRequest request)
        {
            _request = request;
            _entrustPayService = XDI.Resolve<IEntrustPaymentService>();
            _bindInfoService = XDI.Resolve<IAgreePayBankCardBindInfoService>();
        }

        public ObjectResult Invoke()
        {
            String traceService = $"{this.GetType().FullName}.Invoke()";
            String requestService = $"{_request.Method}.{_request.Version}";
            String traceMethod = String.Empty;

            switch (requestService)
            {
                case "cpi.unified.pay.99bill.1.0":
                    var commonPayRequest = JsonUtil.DeserializeObject<CommonPayRequest>(_request.BizContent);
                    if (!commonPayRequest.Success)
                    {
                        _logger.Error(TraceType.ROUTE.ToString(), CallResultStatus.ERROR.ToString(), traceService, requestService, "BizContent解析失败", commonPayRequest.FirstException, _request.BizContent);
                        return new ObjectResult(null, ErrorCode.BIZ_CONTENT_DESERIALIZE_FAILED);
                    }

                    _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), traceService, "BuildCPIEntrustPayPaymentRequest(...)", LogPhase.BEGIN, "开始构造代扣支付请求参数");
                    var unifiedPayRequest = BuildCPIEntrustPayPaymentRequest(commonPayRequest.Value);
                    if (!unifiedPayRequest.Success)
                    {
                        return new ObjectResult(null, unifiedPayRequest.FirstException);
                    }
                    _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), traceService, "BuildCPIEntrustPayPaymentRequest(...)", LogPhase.END, "结束构造代扣支付请求参数");

                    unifiedPayRequest.Value.AppId = _request.AppId;

                    traceMethod = $"{_entrustPayService.GetType().FullName}.Pay(...)";
                    _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), traceService, traceMethod, LogPhase.BEGIN, $"快钱代扣：开始支付", unifiedPayRequest.Value);

                    var unifiedPayResult = _entrustPayService.Pay(unifiedPayRequest.Value);

                    _logger.Trace(TraceType.ROUTE.ToString(), (unifiedPayResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), traceService, traceMethod, LogPhase.END, $"结束支付", unifiedPayResult.Value);

                    return unifiedPayResult.Success ? new ObjectResult(unifiedPayResult.Value) : new ObjectResult(null, unifiedPayResult.ErrorCode, unifiedPayResult.FirstException);
                case "cpi.entrust.pay.99bill.1.0":
                    var entrustPayRequest = JsonUtil.DeserializeObject<CPIEntrustPayPaymentRequest>(_request.BizContent);
                    if (!entrustPayRequest.Success)
                    {
                        _logger.Error(TraceType.ROUTE.ToString(), CallResultStatus.ERROR.ToString(), traceService, requestService, "BizContent解析失败", entrustPayRequest.FirstException, _request.BizContent);
                        return new ObjectResult(null, ErrorCode.BIZ_CONTENT_DESERIALIZE_FAILED);
                    }
                    entrustPayRequest.Value.AppId = _request.AppId;

                    traceMethod = $"{_entrustPayService.GetType().FullName}.Pay(...)";

                    _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), traceService, traceMethod, LogPhase.BEGIN, $"开始支付", entrustPayRequest.Value);

                    var entrustPayResult = _entrustPayService.Pay(entrustPayRequest.Value);

                    _logger.Trace(TraceType.ROUTE.ToString(), (entrustPayResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), traceService, traceMethod, LogPhase.END, $"结束支付", entrustPayResult.Value);

                    return entrustPayResult.Success ? new ObjectResult(entrustPayResult.Value) : new ObjectResult(null, entrustPayResult.ErrorCode, entrustPayResult.FirstException);
            }

            return new ObjectResult(null, ErrorCode.METHOD_NOT_SUPPORT, new NotSupportedException($"method \"{ _request.Method }\" not support"));
        }

        private XResult<CPIEntrustPayPaymentRequest> BuildCPIEntrustPayPaymentRequest(CommonPayRequest request)
        {
            if (request == null)
            {
                return new XResult<CPIEntrustPayPaymentRequest>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentNullException(nameof(request)));
            }

            if (!request.IsValid)
            {
                return new XResult<CPIEntrustPayPaymentRequest>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentException(request.ErrorMessage));
            }

            String service = $"{this.GetType().FullName}.BuildCPIEntrustPayPaymentRequest(...)";

            var queryResult = _bindInfoService.GetBankCardBindDetails(request.PayerId, request.BankCardNo, GlobalConfig.X99bill_PayChannelCode);
            if (!queryResult.Success || queryResult.Value == null || queryResult.Value.Count() == 0)
            {
                _logger.Error(TraceType.ROUTE.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(_bindInfoService)}.GetBankCardBindDetails(...)", "未查询到该用户的绑卡信息", queryResult.FirstException, new
                {
                    request.PayerId,
                    request.BankCardNo,
                    PayChannelCode = GlobalConfig.X99bill_PayChannelCode
                });
                return new XResult<CPIEntrustPayPaymentRequest>(null, ErrorCode.DB_QUERY_FAILED, new DbQueryException("未查询到该用户的绑卡信息"));
            }

            // 默认取第一条绑卡信息
            var boundInfo = queryResult.Value.FirstOrDefault();

            var result = new CPIEntrustPayPaymentRequest()
            {
                PayerId = request.PayerId,
                Amount = request.Amount,
                OutTradeNo = request.OutTradeNo,
                IDCardNo = boundInfo.IDCardNo,
                RealName = boundInfo.RealName,
                BankCardNo = boundInfo.BankCardNo,
                Mobile = boundInfo.Mobile
            };

            return new XResult<CPIEntrustPayPaymentRequest>(result);
        }
    }
}
