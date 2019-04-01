using System;
using System.Collections.Generic;
using System.Text;
using CPI.Common;
using CPI.Common.Domain.SettleDomain.Bill99;
using CPI.IService.SettleServices;
using CPI.Utils;
using ATBase.Core;
using ATBase.Logging;

namespace CPI.Handlers.Settle
{
    internal class Bill99AllotAmountInvocation : IInvocation
    {
        private static readonly ILogger _logger = LogManager.GetLogger();

        private readonly GatewayCommonRequest _request;
        private readonly IAllotAmountService _service;

        public Bill99AllotAmountInvocation(GatewayCommonRequest request)
        {
            _request = request;
            _service = XDI.Resolve<IAllotAmountService>();
        }

        public ObjectResult Invoke()
        {
            String traceService = $"{this.GetType().FullName}.Invoke()";
            String requestService = $"{_request.Method}.{_request.Version}";
            String traceMethod = String.Empty;

            switch (requestService)
            {
                //case "cpi.settle.allot.pay.1.0":
                //    var payRequest = JsonUtil.DeserializeObject<AllotAmountPayRequest>(_request.BizContent);
                //    if (!payRequest.Success)
                //    {
                //        return new ObjectResult(null, payRequest.FirstException);
                //    }

                //    payRequest.Value.AppId = _request.AppId;

                //    traceMethod = $"{_service.GetType().FullName}.Pay(...)";
                //    _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), traceService, traceMethod, LogPhase.BEGIN, "开始消费分账", payRequest.Value);

                //    var payResult = _service.Pay(payRequest.Value);

                //    _logger.Trace(TraceType.ROUTE.ToString(), (payResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), traceService, traceMethod, LogPhase.END, "结束消费分账", payResult.Value);

                //    return payResult.Success ? new ObjectResult(payResult.Value) : new ObjectResult(null, payResult.ErrorCode, payResult.FirstException);
                //case "cpi.settle.allot.refund.1.0":
                //    var refundRequest = JsonUtil.DeserializeObject<AllotAmountRefundRequest>(_request.BizContent);
                //    if (!refundRequest.Success)
                //    {
                //        return new ObjectResult(null, refundRequest.FirstException);
                //    }
                //    refundRequest.Value.AppId = _request.AppId;

                //    traceMethod = $"{_service.GetType().FullName}.Refund(...)";
                //    _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), traceService, traceMethod, LogPhase.BEGIN, "开始退货分账", refundRequest.Value);

                //    var refundResult = _service.Refund(refundRequest.Value);

                //    _logger.Trace(TraceType.ROUTE.ToString(), (refundResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), traceService, traceMethod, LogPhase.END, "结束退货分账", refundResult.Value);

                //    return refundResult.Success ? new ObjectResult(refundResult.Value) : new ObjectResult(null, refundResult.ErrorCode, refundResult.FirstException);
                case "cpi.settle.allot.query.1.0":
                    var queryRequest = JsonUtil.DeserializeObject<AllotAmountResultQueryRequest>(_request.BizContent);
                    if (!queryRequest.Success)
                    {
                        return new ObjectResult(null, queryRequest.FirstException);
                    }
                    queryRequest.Value.AppId = _request.AppId;

                    traceMethod = $"{_service.GetType().FullName}.Query(...)";
                    _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), traceService, traceMethod, LogPhase.BEGIN, "开始查询分账结果", queryRequest.Value);

                    var queryResult = _service.Query(queryRequest.Value);

                    _logger.Trace(TraceType.ROUTE.ToString(), (queryResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), traceService, traceMethod, LogPhase.END, "结束查询分账结果", queryResult.Value);

                    return queryResult.Success ? new ObjectResult(queryResult.Value) : new ObjectResult(null, queryResult.ErrorCode, queryResult.FirstException);
                //case "cpi.settle.allot.period.modify.1.0":
                //    var modifyRequest = JsonUtil.DeserializeObject<SettlementPeriodModifyRequest>(_request.BizContent);
                //    if (!modifyRequest.Success)
                //    {
                //        return new ObjectResult(null, modifyRequest.FirstException);
                //    }
                //    modifyRequest.Value.AppId = _request.AppId;

                //    traceMethod = $"{_service.GetType().FullName}.ModifySettlePeriod(...)";
                //    _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), traceService, traceMethod, LogPhase.BEGIN, "开始修改结算周期", modifyRequest.Value);

                //    var modifyResult = _service.ModifySettlePeriod(modifyRequest.Value);

                //    _logger.Trace(TraceType.ROUTE.ToString(), (modifyResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), traceService, traceMethod, LogPhase.END, "结束修改结算周期", modifyResult.Value);

                //    return modifyResult.Success ? new ObjectResult(modifyResult.Value) : new ObjectResult(null, modifyResult.ErrorCode, modifyResult.FirstException);
            }

            return new ObjectResult(null, ErrorCode.METHOD_NOT_SUPPORT, new NotSupportedException($"method \"{requestService}\" not support"));
        }
    }
}
