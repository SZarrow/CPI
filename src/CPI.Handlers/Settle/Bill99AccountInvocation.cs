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
    internal class Bill99AccountInvocation : IInvocation
    {
        private static readonly ILogger _logger = LogManager.GetLogger();

        private readonly GatewayCommonRequest _request;
        private readonly IAccountService _service;

        public Bill99AccountInvocation(GatewayCommonRequest request)
        {
            _request = request;
            _service = XDI.Resolve<IAccountService>();
        }

        public ObjectResult Invoke()
        {
            String traceService = $"{this.GetType().FullName}.Invoke()";
            String requestService = $"{_request.Method}.{_request.Version}";
            String traceMethod = String.Empty;

            switch (requestService)
            {
                case "cpi.settle.account.balance.query.1.0":
                    var queryRequest = JsonUtil.DeserializeObject<AccountBalanceQueryRequest>(_request.BizContent);
                    if (!queryRequest.Success)
                    {
                        return new ObjectResult(null, queryRequest.FirstException);
                    }
                    queryRequest.Value.AppId = _request.AppId;

                    traceMethod = $"{_service.GetType().FullName}.Pay(...)";
                    _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), traceService, traceMethod, LogPhase.BEGIN, "开始查询账户余额", queryRequest.Value);

                    var queryResult = _service.GetBalance(queryRequest.Value);

                    _logger.Trace(TraceType.ROUTE.ToString(), (queryResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), traceService, traceMethod, LogPhase.END, "结束查询账户余额", queryResult.Value);

                    return queryResult.Success ? new ObjectResult(queryResult.Value) : new ObjectResult(null, queryResult.ErrorCode, queryResult.FirstException);
            }

            return new ObjectResult(null, ErrorCode.METHOD_NOT_SUPPORT, new NotSupportedException($"method \"{requestService}\" not support"));
        }
    }
}
