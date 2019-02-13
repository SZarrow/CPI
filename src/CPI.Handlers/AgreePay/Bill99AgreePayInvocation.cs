using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPI.Common;
using CPI.Common.Domain.AgreePay;
using CPI.Common.Domain.Common;
using CPI.Common.Exceptions;
using CPI.Config;
using CPI.IService.AgreePay;
using CPI.Utils;
using Lotus.Core;
using Lotus.Logging;

namespace CPI.Handlers.AgreePay
{
    internal class Bill99AgreePayInvocation : IInvocation
    {
        private static readonly ILogger _logger = LogManager.GetLogger();

        private readonly GatewayCommonRequest _request;
        private readonly IAgreementPaymentService _agreePayService;
        private readonly IAgreePayBankCardBindInfoService _bindInfoService;

        public Bill99AgreePayInvocation(GatewayCommonRequest request)
        {
            _request = request;
            _agreePayService = XDI.Resolve<IAgreementPaymentService>();
            _bindInfoService = XDI.Resolve<IAgreePayBankCardBindInfoService>();
        }

        public ObjectResult Invoke()
        {
            String traceService = $"{this.GetType().FullName}.Invoke()";
            String requestService = $"{_request.Method}.{_request.Version}";
            String traceMethod = String.Empty;

            switch (requestService)
            {
                case "cpi.agreepay.apply.99bill.1.0":
                    var applyRequest = JsonUtil.DeserializeObject<CPIAgreePayApplyRequest>(_request.BizContent);
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
                case "cpi.agreepay.bindcard.99bill.1.0":
                    var bindRequest = JsonUtil.DeserializeObject<CPIAgreePayBindCardRequest>(_request.BizContent);
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
                case "cpi.unified.pay.99bill.1.0":
                case "cpi.agreepay.pay.99bill.1.0":
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
                case "cpi.agreepay.querystatus.99bill.1.0":
                case "cpi.unified.querystatus.1.0":
                    var queryRequest = JsonUtil.DeserializeObject<CPIAgreePayQueryRequest>(_request.BizContent);
                    if (!queryRequest.Success)
                    {
                        _logger.Error(TraceType.ROUTE.ToString(), CallResultStatus.ERROR.ToString(), traceService, requestService, "BizContent解析失败", queryRequest.FirstException, _request.BizContent);
                        return new ObjectResult(null, ErrorCode.BIZ_CONTENT_DESERIALIZE_FAILED);
                    }
                    queryRequest.Value.AppId = _request.AppId;

                    traceMethod = $"{_agreePayService.GetType().FullName}.Query(...)";

                    _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), traceService, traceMethod, LogPhase.BEGIN, $"开始查询支付状态", queryRequest.Value);

                    var queryResult = _agreePayService.Query(queryRequest.Value);

                    _logger.Trace(TraceType.ROUTE.ToString(), (queryResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), traceService, traceMethod, LogPhase.END, $"结束查询支付状态", queryResult.Value);

                    return queryResult.Success ? new ObjectResult(new PagedListResult<CPIAgreePayQueryResult>()
                    {
                        Items = queryResult.Value,
                        PageIndex = queryResult.Value.PageInfo.PageIndex,
                        PageSize = queryResult.Value.PageInfo.PageSize,
                        TotalCount = queryResult.Value.PageInfo.TotalCount
                    }) : new ObjectResult(null, queryResult.ErrorCode, queryResult.FirstException);
            }

            return new ObjectResult(null, ErrorCode.METHOD_NOT_SUPPORT, new NotSupportedException($"method \"{requestService}\" not support"));
        }

        private XResult<CPIAgreePayPaymentRequest> BuildCPIAgreePayPaymentRequest(CommonPayRequest request)
        {
            if (request == null)
            {
                return new XResult<CPIAgreePayPaymentRequest>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentNullException(nameof(request)));
            }

            if (!request.IsValid)
            {
                return new XResult<CPIAgreePayPaymentRequest>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentException(request.ErrorMessage));
            }

            var queryResult = _bindInfoService.GetBankCardBindDetails(request.PayerId, request.BankCardNo, GlobalConfig.X99bill_PayChannelCode);
            if (!queryResult.Success || queryResult.Value == null || queryResult.Value.Count() == 0)
            {
                _logger.Error(TraceType.ROUTE.ToString(), CallResultStatus.ERROR.ToString(), $"BuildCPIAgreePayPaymentRequest(...)", "构造协议支付请求参数", "未查询到该用户的绑卡信息", queryResult.FirstException, new
                {
                    request.PayerId,
                    request.BankCardNo,
                    PayChannelCode = GlobalConfig.X99bill_PayChannelCode
                });
                return new XResult<CPIAgreePayPaymentRequest>(null, ErrorCode.NO_BANKCARD_BOUND, new DbQueryException("未查询到该用户的绑卡信息"));
            }

            // 默认取第一条绑卡信息
            var boundInfo = queryResult.Value.FirstOrDefault();

            var result = new CPIAgreePayPaymentRequest()
            {
                PayerId = request.PayerId,
                Amount = request.Amount,
                OutTradeNo = request.OutTradeNo,
                BankCardNo = boundInfo.BankCardNo,
                PayToken = boundInfo.PayToken,
                Mobile = boundInfo.Mobile,
                SharingType = request.SharingType,
                SharingPeriod = request.SharingPeriod,
                FeePayerId = request.FeePayerId,
                Fee = request.Fee
            };

            return new XResult<CPIAgreePayPaymentRequest>(result);
        }
    }
}
