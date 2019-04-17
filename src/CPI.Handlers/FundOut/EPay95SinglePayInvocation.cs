using System;
using System.Collections.Generic;
using System.Text;
using CPI.Common;
using CPI.Common.Domain.Common;
using CPI.Common.Domain.FundOut.EPay95;
using CPI.IService.FundOut;
using CPI.Utils;
using ATBase.Core;
using ATBase.Logging;

namespace CPI.Handlers.FundOut
{
    /// <summary>
    /// 双乾单笔代付
    /// </summary>
    internal class EPay95SinglePayInvocation : IInvocation
    {
        private static readonly ILogger _logger = LogManager.GetLogger();

        private readonly GatewayCommonRequest _request;
        private readonly IEPay95SinglePaymentService _service;

        public EPay95SinglePayInvocation(GatewayCommonRequest request)
        {
            _request = request;
            _service = XDI.Resolve<IEPay95SinglePaymentService>();
        }

        public ObjectResult Invoke()
        {
            String traceService = $"{this.GetType().FullName}.{nameof(Invoke)}()";
            String requestService = $"{_request.Method}.{_request.Version}";
            String traceMethod = String.Empty;

            switch (requestService)
            {
                case "cpi.fundout.single.95epay.pay.1.0":
                    var payRequest = JsonUtil.DeserializeObject<PayRequest>(_request.BizContent);
                    if (!payRequest.Success)
                    {
                        _logger.Error(TraceType.ROUTE.ToString(), CallResultStatus.ERROR.ToString(), traceService, requestService, "BizContent解析失败", payRequest.FirstException, _request.BizContent);
                        return new ObjectResult(null, ErrorCode.BIZ_CONTENT_DESERIALIZE_FAILED);
                    }
                    payRequest.Value.AppId = _request.AppId;

                    traceMethod = $"{_service.GetType().FullName}.{nameof(_service.Pay)}(...)";
                    _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), traceService, traceMethod, LogPhase.BEGIN, "开始支付", payRequest.Value);

                    var payResult = _service.Pay(payRequest.Value);

                    _logger.Trace(TraceType.ROUTE.ToString(), (payResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), traceService, traceMethod, LogPhase.END, "结束支付", payResult.Value);

                    return payResult.Success ? new ObjectResult(payResult.Value) : new ObjectResult(null, payResult.ErrorCode, payResult.FirstException);
                case "cpi.fundout.single.95epay.querystatus.1.0":
                    var queryStatusRequest = JsonUtil.DeserializeObject<QueryRequest>(_request.BizContent);
                    if (!queryStatusRequest.Success)
                    {
                        _logger.Error(TraceType.ROUTE.ToString(), CallResultStatus.ERROR.ToString(), traceService, requestService, "BizContent解析失败", queryStatusRequest.FirstException, _request.BizContent);
                        return new ObjectResult(null, ErrorCode.BIZ_CONTENT_DESERIALIZE_FAILED);
                    }
                    queryStatusRequest.Value.AppId = _request.AppId;

                    traceMethod = $"{_service.GetType().FullName}.{nameof(_service.QueryStatus)}(...)";
                    _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), traceService, traceMethod, LogPhase.BEGIN, "开始查询支付结果状态", queryStatusRequest.Value);

                    var queryStatusResult = _service.QueryStatus(queryStatusRequest.Value);

                    _logger.Trace(TraceType.ROUTE.ToString(), (queryStatusResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), traceService, traceMethod, LogPhase.END, "结束查询支付结果状态", queryStatusResult.Value);

                    return queryStatusResult.Success
                        ? new ObjectResult(new PagedListResult<QueryStatusResult>()
                        {
                            Items = queryStatusResult.Value,
                            PageIndex = queryStatusResult.Value.PageInfo.PageIndex,
                            PageSize = queryStatusResult.Value.PageInfo.PageSize,
                            TotalCount = queryStatusResult.Value.PageInfo.TotalCount
                        })
                        : new ObjectResult(null, queryStatusResult.ErrorCode, queryStatusResult.FirstException);
                case "cpi.fundout.single.95epay.querydetail.1.0":
                    var queryDetailRequest = JsonUtil.DeserializeObject<QueryRequest>(_request.BizContent);
                    if (!queryDetailRequest.Success)
                    {
                        _logger.Error(TraceType.ROUTE.ToString(), CallResultStatus.ERROR.ToString(), traceService, requestService, "BizContent解析失败", queryDetailRequest.FirstException, _request.BizContent);
                        return new ObjectResult(null, ErrorCode.BIZ_CONTENT_DESERIALIZE_FAILED);
                    }
                    queryDetailRequest.Value.AppId = _request.AppId;

                    traceMethod = $"{_service.GetType().FullName}.{nameof(_service.QueryDetails)}(...)";
                    _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), traceService, traceMethod, LogPhase.BEGIN, "开始查询支付结果详情", queryDetailRequest.Value);

                    var queryResult = _service.QueryDetails(queryDetailRequest.Value);

                    _logger.Trace(TraceType.ROUTE.ToString(), (queryResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), traceService, traceMethod, LogPhase.END, "结束查询支付结果详情", queryResult.Value);

                    return queryResult.Success
                        ? new ObjectResult(new PagedListResult<QueryDetailResult>()
                        {
                            Items = queryResult.Value,
                            PageIndex = queryResult.Value.PageInfo.PageIndex,
                            PageSize = queryResult.Value.PageInfo.PageSize,
                            TotalCount = queryResult.Value.PageInfo.TotalCount
                        })
                        : new ObjectResult(null, queryResult.ErrorCode, queryResult.FirstException);
            }

            return new ObjectResult(null, ErrorCode.METHOD_NOT_SUPPORT, new NotSupportedException($"不支持服务\"{requestService}\""));
        }
    }
}
