using System;
using System.Collections.Generic;
using System.Text;
using CPI.Common;
using CPI.Common.Domain.FundOut.YeePay;
using CPI.IService.FundOut;
using CPI.Utils;
using Lotus.Core;
using Lotus.Logging;

namespace CPI.Handlers.FundOut
{
    /// <summary>
    /// 易宝支付单笔代付
    /// </summary>
    internal class YeePaySinglePayInvocation : IInvocation
    {
        private static readonly ILogger _logger = LogManager.GetLogger();

        private readonly GatewayCommonRequest _request;
        private readonly IYeePaySinglePaymentService _service;

        public YeePaySinglePayInvocation(GatewayCommonRequest request)
        {
            _request = request;
            _service = XDI.Resolve<IYeePaySinglePaymentService>();
        }

        public ObjectResult Invoke()
        {
            String traceService = $"{this.GetType().FullName}.Invoke()";
            String requestService = $"{_request.Method}.{_request.Version}";
            String traceMethod = String.Empty;

            switch (requestService)
            {
                case "cpi.fundout.single.yeepay.pay.1.0":
                    return Pay_1_0(traceService, requestService, ref traceMethod);
                case "cpi.fundout.single.yeepay.querystatus.1.0":
                    return QueryStatus_1_0(traceService, requestService, ref traceMethod);
            }

            return new ObjectResult(null, ErrorCode.METHOD_NOT_SUPPORT, new NotSupportedException($"method \"{requestService}\" not support"));
        }

        private ObjectResult Pay_1_0(String traceService, String requestService, ref String traceMethod)
        {
            var payRequest = JsonUtil.DeserializeObject<YeePaySinglePayRequest>(_request.BizContent);
            if (!payRequest.Success)
            {
                _logger.Error(TraceType.ROUTE.ToString(), CallResultStatus.ERROR.ToString(), traceService, requestService, "BizContent解析失败", payRequest.FirstException, _request.BizContent);
                return new ObjectResult(0, ErrorCode.BIZ_CONTENT_DESERIALIZE_FAILED);
            }
            payRequest.Value.AppId = _request.AppId;

            if (!payRequest.Value.IsValid)
            {
                return new ObjectResult(0, ErrorCode.INVALID_ARGUMENT, new ArgumentException(payRequest.Value.ErrorMessage));
            }

            traceMethod = $"{_service.GetType().FullName}.Pull(...)";

            _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), traceService, traceMethod, LogPhase.BEGIN, $"开始执行代付", payRequest.Value);

            var payResult = _service.Pay(payRequest.Value);

            _logger.Trace(TraceType.ROUTE.ToString(), (payResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), traceService, traceMethod, LogPhase.END, $"结束执行代付", payResult.Value);

            return payResult.Success ? new ObjectResult(payResult.Value) : new ObjectResult(null, payResult.ErrorCode, payResult.FirstException);
        }

        private ObjectResult QueryStatus_1_0(String traceService, String requestService, ref String traceMethod)
        {
            throw new NotImplementedException();
        }
    }
}
