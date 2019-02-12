using System;
using System.Collections.Generic;
using System.Text;
using CPI.Common;
using CPI.Common.Domain.Common;
using CPI.Common.Domain.FundOut.Bill99;
using CPI.IService.FundOut;
using CPI.Utils;
using Lotus.Core;
using Lotus.Logging;

namespace CPI.Handlers.FundOut
{
    internal class Bill99SinglePayInvocation : IInvocation
    {
        private static readonly ILogger _logger = LogManager.GetLogger();

        private readonly GatewayCommonRequest _request;
        private readonly IBill99SinglePaymentService _service = null;

        public Bill99SinglePayInvocation(GatewayCommonRequest request)
        {
            _request = request;
            _service = XDI.Resolve<IBill99SinglePaymentService>();
        }

        public ObjectResult Invoke()
        {
            String traceService = $"{this.GetType().FullName}.Invoke()";
            String requestService = $"{_request.Method}.{_request.Version}";
            String traceMethod = String.Empty;

            switch (requestService)
            {
                case "cpi.fundout.single.99bill.pay.1.0":
                    var payRequest = JsonUtil.DeserializeObject<SingleSettlementPaymentApplyRequest>(_request.BizContent);
                    if (!payRequest.Success)
                    {
                        _logger.Error(TraceType.ROUTE.ToString(), CallResultStatus.ERROR.ToString(), traceService, requestService, "BizContent解析失败", payRequest.FirstException, _request.BizContent);
                        return new ObjectResult(null, ErrorCode.BIZ_CONTENT_DESERIALIZE_FAILED);
                    }
                    payRequest.Value.AppId = _request.AppId;

                    traceMethod = $"{_service.GetType().FullName}.Pay(...)";
                    _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), traceService, traceMethod, LogPhase.BEGIN, "开始支付", payRequest.Value);

                    var payResult = _service.Pay(payRequest.Value);

                    _logger.Trace(TraceType.ROUTE.ToString(), (payResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), traceService, traceMethod, LogPhase.END, "结束支付", payResult.Value);

                    return payResult.Success ? new ObjectResult(payResult.Value) : new ObjectResult(null, payResult.ErrorCode, payResult.FirstException);
                case "cpi.fundout.single.99bill.query.1.0":
                    var queryRequest = JsonUtil.DeserializeObject<SingleSettlementQueryRequest>(_request.BizContent);
                    if (!queryRequest.Success)
                    {
                        _logger.Error(TraceType.ROUTE.ToString(), CallResultStatus.ERROR.ToString(), traceService, requestService, "BizContent解析失败", queryRequest.FirstException, _request.BizContent);
                        return new ObjectResult(null, ErrorCode.BIZ_CONTENT_DESERIALIZE_FAILED);
                    }

                    traceMethod = $"{_service.GetType().FullName}.Query(...)";
                    _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), traceService, traceMethod, LogPhase.BEGIN, "开始查询支付结果", queryRequest.Value);

                    var queryResult = _service.Query(queryRequest.Value);

                    _logger.Trace(TraceType.ROUTE.ToString(), (queryResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), traceService, traceMethod, LogPhase.END, $"结束查询支付结果", queryResult.Value);

                    return queryResult.Success
                        ? new ObjectResult(new PagedListResult<SingleSettlementQueryResponse>()
                        {
                            Items = queryResult.Value,
                            PageIndex = queryResult.Value.PageInfo.PageIndex,
                            PageSize = queryResult.Value.PageInfo.PageSize,
                            TotalCount = queryResult.Value.PageInfo.TotalCount
                        })
                        : new ObjectResult(null, queryResult.ErrorCode, queryResult.FirstException);
                case "cpi.fundout.single.99bill.querystatus.1.0":
                    var queryStatusRequest = JsonUtil.DeserializeObject<SingleSettlementQueryRequest>(_request.BizContent);
                    if (!queryStatusRequest.Success)
                    {
                        _logger.Error(TraceType.ROUTE.ToString(), CallResultStatus.ERROR.ToString(), traceService, requestService, "BizContent解析失败", queryStatusRequest.FirstException, _request.BizContent);
                        return new ObjectResult(null, ErrorCode.BIZ_CONTENT_DESERIALIZE_FAILED);
                    }

                    traceMethod = $"{_service.GetType().FullName}.Query(...)";
                    _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), traceService, traceMethod, LogPhase.BEGIN, "开始查询支付状态", queryStatusRequest.Value);

                    var queryStatusResult = _service.QueryStatus(queryStatusRequest.Value);

                    _logger.Trace(TraceType.ROUTE.ToString(), (queryStatusResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), traceService, traceMethod, LogPhase.END, "结束查询支付状态", queryStatusResult.Value);

                    return queryStatusResult.Success
                        ? new ObjectResult(new PagedListResult<OrderStatusResult>()
                        {
                            Items = queryStatusResult.Value,
                            PageIndex = queryStatusResult.Value.PageInfo.PageIndex,
                            PageSize = queryStatusResult.Value.PageInfo.PageSize,
                            TotalCount = queryStatusResult.Value.PageInfo.TotalCount
                        })
                        : new ObjectResult(null, queryStatusResult.ErrorCode, queryStatusResult.FirstException);
            }

            return new ObjectResult(null, ErrorCode.METHOD_NOT_SUPPORT, new NotSupportedException($"method \"{requestService}\" not support"));
        }
    }
}
