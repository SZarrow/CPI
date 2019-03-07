﻿using System;
using System.Collections.Generic;
using System.Text;
using CPI.Common;
using CPI.Common.Domain.AgreePay;
using CPI.Common.Domain.Common;
using CPI.IService.AgreePay;
using CPI.Utils;
using Lotus.Core;
using Lotus.Logging;

namespace CPI.Handlers.AgreePay
{
    public class AgreePayInvocation : IInvocation
    {
        private static readonly ILogger _logger = LogManager.GetLogger();

        private readonly GatewayCommonRequest _request;
        private readonly IAgreementPaymentService _agreePayService;
        private readonly IAgreePayBankCardBindInfoService _bindInfoService;

        public AgreePayInvocation(GatewayCommonRequest request)
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
    }
}
