using System;
using System.Linq;
using CPI.Common;
using CPI.Common.Domain.AgreePay;
using CPI.Common.Domain.AgreePay.YeePay;
using CPI.Common.Domain.Common;
using CPI.Common.Domain.SettleDomain.Bill99.v1_0;
using CPI.Common.Exceptions;
using CPI.Config;
using CPI.IService.AgreePay;
using CPI.Utils;
using Lotus.Core;
using Lotus.Logging;

namespace CPI.Handlers.AgreePay
{
    public class YeePayAgreePayInvocation : IInvocation
    {
        private static readonly ILogger _logger = LogManager.GetLogger();

        private readonly GatewayCommonRequest _request;
        private readonly IYeePayAgreementPaymentService _agreePayService;
        private readonly IAgreePayBankCardBindInfoService _bindInfoService;

        public YeePayAgreePayInvocation(GatewayCommonRequest request)
        {
            _request = request;
            _agreePayService = XDI.Resolve<IYeePayAgreementPaymentService>();
            _bindInfoService = XDI.Resolve<IAgreePayBankCardBindInfoService>();
        }

        public ObjectResult Invoke()
        {
            String traceService = $"{this.GetType().FullName}.Invoke()";
            String requestService = $"{_request.Method}.{_request.Version}";
            String traceMethod = String.Empty;

            switch (requestService)
            {
                case "cpi.agreepay.apply.yeepay.1.0":
                    return ApplyBindCard_1_0(traceService, requestService, ref traceMethod);
                case "cpi.agreepay.bindcard.yeepay.1.0":
                    return BindCard_1_0(traceService, requestService, ref traceMethod);
                case "cpi.unified.pay.yeepay.1.0":
                case "cpi.agreepay.pay.yeepay.1.0":
                    return Pay_1_0(traceService, requestService, ref traceMethod);
                case "cpi.agreepay.refund.yeepay.1.0":
                    return Refund_1_0(traceService, requestService, ref traceMethod);
                case "cpi.agreepay.payresult.pull.yeepay.1.0":
                    return PayResultPull_1_0(traceService, requestService, ref traceMethod);
                case "cpi.agreepay.refundresult.pull.yeepay.1.0":
                    return RefundResultPull_1_0(traceService, requestService, ref traceMethod);
            }

            return new ObjectResult(null, ErrorCode.METHOD_NOT_SUPPORT, new NotSupportedException($"method \"{requestService}\" not support"));
        }

        private ObjectResult BindCard_1_0(String traceService, String requestService, ref String traceMethod)
        {
            var bindRequest = JsonUtil.DeserializeObject<YeePayAgreePayBindCardRequest>(_request.BizContent);
            if (!bindRequest.Success)
            {
                _logger.Error(TraceType.ROUTE.ToString(), CallResultStatus.ERROR.ToString(), traceService, requestService, "BizContent解析失败", bindRequest.FirstException, _request.BizContent);
                return new ObjectResult(null, ErrorCode.BIZ_CONTENT_DESERIALIZE_FAILED);
            }
            bindRequest.Value.AppId = _request.AppId;

            traceMethod = $"{_agreePayService.GetType().FullName}.BindCard(...)";

            _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), traceService, traceMethod, LogPhase.BEGIN, "开始绑卡", bindRequest.Value);

            var bindResult = _agreePayService.BindCard(bindRequest.Value);

            _logger.Trace(TraceType.ROUTE.ToString(), (bindResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), traceService, traceMethod, LogPhase.END, "结束绑卡", bindResult.Value);

            return bindResult.Success ? new ObjectResult(bindResult.Value) : new ObjectResult(null, bindResult.ErrorCode, bindResult.FirstException);
        }

        private ObjectResult ApplyBindCard_1_0(String traceService, String requestService, ref String traceMethod)
        {
            var applyRequest = JsonUtil.DeserializeObject<YeePayAgreePayApplyRequest>(_request.BizContent);
            if (!applyRequest.Success)
            {
                _logger.Error(TraceType.ROUTE.ToString(), CallResultStatus.ERROR.ToString(), traceService, requestService, "BizContent解析失败", applyRequest.FirstException, _request.BizContent);
                return new ObjectResult(null, ErrorCode.BIZ_CONTENT_DESERIALIZE_FAILED);
            }
            applyRequest.Value.AppId = _request.AppId;

            traceMethod = $"{_agreePayService.GetType().FullName}.Apply(...)";

            _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), traceService, traceMethod, LogPhase.BEGIN, "开始申请绑卡", applyRequest.Value);

            var applyResult = _agreePayService.Apply(applyRequest.Value);

            _logger.Trace(TraceType.ROUTE.ToString(), (applyResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), traceService, traceMethod, LogPhase.END, "结束申请绑卡", applyResult.Value);

            return applyResult.Success ? new ObjectResult(applyResult.Value) : new ObjectResult(null, applyResult.ErrorCode, applyResult.FirstException);
        }

        private ObjectResult Pay_1_0(String traceService, String requestService, ref String traceMethod)
        {
            var commonPayRequest = JsonUtil.DeserializeObject<CommonPayRequest>(_request.BizContent);
            if (!commonPayRequest.Success)
            {
                _logger.Error(TraceType.ROUTE.ToString(), CallResultStatus.ERROR.ToString(), traceService, requestService, "BizContent解析失败", commonPayRequest.FirstException, _request.BizContent);
                return new ObjectResult(null, ErrorCode.BIZ_CONTENT_DESERIALIZE_FAILED);
            }

            _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), traceService, "BuildCPIAgreePayPaymentRequest(...)", LogPhase.BEGIN, "开始构造协议支付请求参数", commonPayRequest.Value);
            var agreePayRequest = BuildCPIAgreePayPaymentRequest(commonPayRequest.Value);
            if (!agreePayRequest.Success)
            {
                return new ObjectResult(null, agreePayRequest.ErrorCode, agreePayRequest.FirstException);
            }
            _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), traceService, "BuildCPIAgreePayPaymentRequest(...)", LogPhase.END, "结束构造协议支付请求参数", agreePayRequest.Value);

            agreePayRequest.Value.AppId = _request.AppId;

            traceMethod = $"{_agreePayService.GetType().FullName}.Pay(...)";

            _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), traceService, traceMethod, LogPhase.BEGIN, $"开始支付", agreePayRequest.Value);

            var agreePayResult = _agreePayService.Pay(agreePayRequest.Value);

            _logger.Trace(TraceType.ROUTE.ToString(), (agreePayResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), traceService, traceMethod, LogPhase.END, "结束支付", agreePayResult.Value);

            return agreePayResult.Success ? new ObjectResult(agreePayResult.Value) : new ObjectResult(null, agreePayResult.ErrorCode, agreePayResult.FirstException);
        }

        private ObjectResult Refund_1_0(String traceService, String requestService, ref String traceMethod)
        {
            var refundRequest = JsonUtil.DeserializeObject<YeePayAgreePayRefundRequest>(_request.BizContent);
            if (!refundRequest.Success)
            {
                _logger.Error(TraceType.ROUTE.ToString(), CallResultStatus.ERROR.ToString(), traceService, requestService, "BizContent解析失败", refundRequest.FirstException, _request.BizContent);
                return new ObjectResult(null, ErrorCode.BIZ_CONTENT_DESERIALIZE_FAILED);
            }

            refundRequest.Value.AppId = _request.AppId;

            traceMethod = $"{_agreePayService.GetType().FullName}.Refund(...)";

            _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), traceService, traceMethod, LogPhase.BEGIN, $"开始退款", refundRequest.Value);

            var refundResult = _agreePayService.Refund(refundRequest.Value);

            _logger.Trace(TraceType.ROUTE.ToString(), (refundResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), traceService, traceMethod, LogPhase.END, "结束退款", refundResult.Value);

            return refundResult.Success ? new ObjectResult(refundResult.Value) : new ObjectResult(null, refundResult.ErrorCode, refundResult.FirstException);
        }

        private ObjectResult PayResultPull_1_0(String traceService, String requestService, ref String traceMethod)
        {
            var pullRequest = JsonUtil.DeserializeObject<CommonPullRequest>(_request.BizContent);
            if (!pullRequest.Success)
            {
                _logger.Error(TraceType.ROUTE.ToString(), CallResultStatus.ERROR.ToString(), traceService, requestService, "BizContent解析失败", pullRequest.FirstException, _request.BizContent);
                return new ObjectResult(0, ErrorCode.BIZ_CONTENT_DESERIALIZE_FAILED);
            }
            pullRequest.Value.AppId = _request.AppId;

            if (!pullRequest.Value.IsValid)
            {
                return new ObjectResult(0, ErrorCode.INVALID_ARGUMENT, new ArgumentException(pullRequest.Value.ErrorMessage));
            }

            traceMethod = $"{_agreePayService.GetType().FullName}.PullPayStatus(...)";

            _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), traceService, traceMethod, LogPhase.BEGIN, $"开始拉取支付状态", pullRequest.Value);

            var pullResult = _agreePayService.PullPayStatus(pullRequest.Value.Count);

            _logger.Trace(TraceType.ROUTE.ToString(), (pullResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), traceService, traceMethod, LogPhase.END, $"结束拉取支付状态", pullResult.Value);

            return pullResult.Success ? new ObjectResult(new CommonPullResponse()
            {
                SuccessCount = pullResult.Value
            }) : new ObjectResult(null, pullResult.ErrorCode, pullResult.FirstException);
        }

        private ObjectResult RefundResultPull_1_0(String traceService, String requestService, ref String traceMethod)
        {
            var pullRequest = JsonUtil.DeserializeObject<CommonPullRequest>(_request.BizContent);
            if (!pullRequest.Success)
            {
                _logger.Error(TraceType.ROUTE.ToString(), CallResultStatus.ERROR.ToString(), traceService, requestService, "BizContent解析失败", pullRequest.FirstException, _request.BizContent);
                return new ObjectResult(0, ErrorCode.BIZ_CONTENT_DESERIALIZE_FAILED);
            }
            pullRequest.Value.AppId = _request.AppId;

            if (!pullRequest.Value.IsValid)
            {
                return new ObjectResult(0, ErrorCode.INVALID_ARGUMENT, new ArgumentException(pullRequest.Value.ErrorMessage));
            }

            traceMethod = $"{_agreePayService.GetType().FullName}.PullRefundStatus(...)";

            _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), traceService, traceMethod, LogPhase.BEGIN, $"开始拉取退款结果", pullRequest.Value);

            var pullResult = _agreePayService.PullRefundStatus(pullRequest.Value.Count);

            _logger.Trace(TraceType.ROUTE.ToString(), (pullResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), traceService, traceMethod, LogPhase.END, $"结束拉取退款结果", pullResult.Value);

            return pullResult.Success ? new ObjectResult(new CommonPullResponse()
            {
                SuccessCount = pullResult.Value
            }) : new ObjectResult(null, pullResult.ErrorCode, pullResult.FirstException);
        }

        private XResult<YeePayAgreePayPaymentRequest> BuildCPIAgreePayPaymentRequest(CommonPayRequest request)
        {
            if (request == null)
            {
                return new XResult<YeePayAgreePayPaymentRequest>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentNullException(nameof(request)));
            }

            if (!request.IsValid)
            {
                return new XResult<YeePayAgreePayPaymentRequest>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentException(request.ErrorMessage));
            }

            var queryResult = _bindInfoService.GetBankCardBindDetails(request.PayerId, request.BankCardNo, GlobalConfig.YEEPAY_PAYCHANNEL_CODE);
            if (!queryResult.Success || queryResult.Value == null || queryResult.Value.Count() == 0)
            {
                _logger.Error(TraceType.ROUTE.ToString(), CallResultStatus.ERROR.ToString(), $"BuildCPIAgreePayPaymentRequest(...)", "构造协议支付请求参数", "未查询到该用户的绑卡信息", queryResult.FirstException, new
                {
                    request.PayerId,
                    request.BankCardNo,
                    PayChannelCode = GlobalConfig.YEEPAY_PAYCHANNEL_CODE
                });
                return new XResult<YeePayAgreePayPaymentRequest>(null, ErrorCode.NO_BANKCARD_BOUND, new DbQueryException("未查询到该用户的绑卡信息"));
            }

            // 默认取第一条绑卡信息
            var boundInfo = queryResult.Value.FirstOrDefault();

            var result = new YeePayAgreePayPaymentRequest()
            {
                PayerId = request.PayerId,
                Amount = request.Amount,
                OutTradeNo = request.OutTradeNo,
                BankCardNo = boundInfo.BankCardNo,
                TerminalNo = request.TerminalNo
            };

            return new XResult<YeePayAgreePayPaymentRequest>(result);
        }
    }
}
