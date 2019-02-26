using System;
using System.Collections.Generic;
using System.Text;
using CPI.Common;
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

        public ObjectResult Invoke()
        {
            String traceService = $"{this.GetType().FullName}.Invoke()";
            String requestService = $"{_request.Method}.{_request.Version}";
            String traceMethod = String.Empty;

            switch (requestService)
            {

            }

            return new ObjectResult(null, ErrorCode.METHOD_NOT_SUPPORT, new NotSupportedException($"method \"{requestService}\" not support"));
        }
    }
}
