using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using CPI.Common;
using CPI.Common.Domain.FundOut.EPay95;
using CPI.IService.FundOut;
using CPI.Utils;
using Lotus.Logging;
using Lotus.Security;
using Lotus.Web.Mvc.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


namespace CPI.WebAPI.Controllers
{
    [Route("[action]/[controller].c")]
    [ApiController]
    public class NotifyController : ControllerBase
    {
        private static readonly ILogger _logger = LogManager.GetLogger();
        private static readonly IActionResult DefaultPostbackSuccessResult = new ContentResult()
        {
            Content = "SUCCESS",
            ContentType = "text/plain"
        };
        private static readonly IActionResult DefaultPostbackFailureResult = new ContentResult()
        {
            Content = "FAILURE",
            ContentType = "text/plain"
        };

        private IEPay95SinglePaymentService _epay95FundOutPaymentService = null;

        [HttpPost]
        public IActionResult EPay95([FromForm]PayNotifyResult request)
        {
            String service = $"{this.GetType().FullName}.EPay95()";

            if (request == null)
            {
                _logger.Trace(TraceType.API.ToString(), CallResultStatus.ERROR.ToString(), service, "request", LogPhase.ACTION, "通知参数为null");
                return DefaultPostbackFailureResult;
            }

            String requestHost = $"{Request.HttpContext.Connection.RemoteIpAddress}:{Request.HttpContext.Connection.RemotePort}";

            _logger.Trace(TraceType.API.ToString(), CallResultStatus.OK.ToString(), service, "双乾代付通知", LogPhase.ACTION, "收到通知", new Object[] { requestHost, request });

            if (!request.IsValid)
            {
                _logger.Error(TraceType.API.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(request)}.IsValid", $"通知参数验证失败：{request.ErrorMessage}");
                return DefaultPostbackFailureResult;
            }

            //将LoanJsonList参数解码
            request.LoanJsonList = HttpUtility.UrlDecode(request.LoanJsonList);
            if (!EPay95Util.VerifySign(request))
            {
                _logger.Error(TraceType.API.ToString(), CallResultStatus.ERROR.ToString(), service, "EPay95Util.VerifySign(...)", "通知参数验签失败", null, request);
                return DefaultPostbackFailureResult;
            }

            String traceMethod = $"{nameof(_epay95FundOutPaymentService)}.UpdatePayStatus(...)";

            _logger.Trace(TraceType.API.ToString(), CallResultStatus.OK.ToString(), service, traceMethod, LogPhase.BEGIN);

            var updateResult = _epay95FundOutPaymentService.UpdatePayStatus(request);

            _logger.Trace(TraceType.API.ToString(), (updateResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), service, traceMethod, LogPhase.END);

            if (!updateResult.Success)
            {
                _logger.Error(TraceType.API.ToString(), CallResultStatus.ERROR.ToString(), service, "双乾代付通知", "处理双乾代付通知失败", updateResult.FirstException, request);
                return DefaultPostbackFailureResult;
            }

            return DefaultPostbackSuccessResult;
        }
    }
}