using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using CPI.Common;
using CPI.Config;
using CPI.Handlers;
using CPI.IService.BaseServices;
using CPI.Security;
using CPI.Utils;
using Lotus.Core;
using Lotus.Logging;
using Lotus.Security;
using Microsoft.AspNetCore.Mvc;

namespace CPI.WebAPI.Controllers
{
    [Route("[controller].c")]
    [ApiController]
    public class GatewayController : ControllerBase
    {
        private static readonly ILogger _logger = LogManager.GetLogger();
        private static readonly String _typeFullName = typeof(GatewayController).FullName;

        private readonly ISysAppService _sysAppService = null;

        [HttpGet]
        public IActionResult Get()
        {
            return Content($"<h1>[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] : CPI is <span style=\"color:green\">Running</span>...</h1>", "text/html");
        }

        [HttpPost]
        public IActionResult Post()
        {
            var request = Request.HasFormContentType ? new GatewayCommonRequest(Request.Form) : new GatewayCommonRequest(Request.Body);

            String service = $"{_typeFullName}.Post()";

            _logger.Trace(TraceType.API.ToString(), CallResultStatus.OK.ToString(), service, ":", LogPhase.ACTION, "CPI.Gateway入站请求", request);

            if (!request.IsValid)
            {
                _logger.Trace(TraceType.API.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(request)}.IsValid", LogPhase.ACTION, $"请求参数验证失败：{request.ErrorMessage}", request);
                return this.Failure(request.AppId, ErrorCode.INVALID_ARGUMENT, request.ErrorMessage);
            }

            if (DateTime.TryParse(request.Timestamp, out DateTime requestTime))
            {
                var now = DateTime.Now;
                if (requestTime < now.AddMinutes(-10) || requestTime > now.AddMinutes(10))
                {
                    return this.Failure(request.AppId, ErrorCode.INVALID_ARGUMENT, "发送请求的时间不正确");
                }
            }

            if (!VerifySign(request))
            {
                _logger.Trace(TraceType.API.ToString(), CallResultStatus.ERROR.ToString(), service, "VerifySign(...)", LogPhase.ACTION, "请求参数验签失败");
                return this.Failure(request.AppId, ErrorCode.SIGN_VERIFY_FAILED);
            }

            var invoker = ProxyActivator.GetInvocation(request);
            if (invoker == null)
            {
                _logger.Trace(TraceType.API.ToString(), CallResultStatus.ERROR.ToString(), service, "ProxyActivator.GetInvocation(...)", LogPhase.ACTION, $"不支持的请求方法：{request.Method}");
                return this.Failure(request.AppId, ErrorCode.METHOD_NOT_SUPPORT);
            }

            String invokerName = invoker.GetType().FullName;

            _logger.Trace(TraceType.API.ToString(), CallResultStatus.OK.ToString(), service, $"{invokerName}.Invoke()", LogPhase.BEGIN, "开始服务调用");

            var invokeResult = invoker.Invoke();

            _logger.Trace(TraceType.API.ToString(), CallResultStatus.OK.ToString(), service, $"{invokerName}.Invoke()", LogPhase.END, "结束服务调用");

            return !invokeResult.Success
                 ? this.Failure(request.AppId, invokeResult.ErrorCode, invokeResult.ErrorMessage)
                 : this.Success(request.AppId, invokeResult.Value);
        }

        private Boolean VerifySign(GatewayCommonRequest request)
        {
            var verifyResult = SignUtil.VerifySign(request.Sign, request.BizContent, GetPublicKey(request.AppId), request.SignType.ToString());
            return verifyResult.Success && verifyResult.Value;
        }

        private String GetPublicKey(String appId)
        {
            return _sysAppService.GetRSAPublicKey(appId);
        }

        private IActionResult Failure(String appId, Int32 errorCode, String errorMessage = null)
        {
            var resp = new GatewayCommonResponse()
            {
                Status = errorCode,
                Msg = errorMessage.HasValue() ? errorMessage : ErrorCodeDescriptor.GetDescription(errorCode),
                Content = new { AppId = appId }
            };

            String callResultStatus = CallResultStatus.ERROR.ToString();

            var respContent = JsonUtil.SerializeObject(resp.Content);
            if (!respContent.Success)
            {
                resp.Status = ErrorCode.SERIALIZE_FAILED;
                resp.Msg = ErrorCodeDescriptor.GetDescription(resp.Status);
                return this.Json(resp, callResultStatus);
            }

            var signResult = CryptoHelper.MakeSign(respContent.Value, KeyConfig.CPICommonPrivateKey, PrivateKeyFormat.PKCS8, HashAlgorithmName.SHA1);
            if (!signResult.Success)
            {
                _logger.Error(TraceType.API.ToString(), CallResultStatus.ERROR.ToString(), $"{_typeFullName}.Failure()", "CryptoHelper.MakeSign(...)", "生成签名失败", signResult.FirstException);

                resp.Status = ErrorCode.SIGN_FAILED;
                resp.Msg = ErrorCodeDescriptor.GetDescription(resp.Status);
                return this.Json(resp, callResultStatus);
            }

            resp.Sign = signResult.Value;
            return this.Json(resp, callResultStatus);
        }

        private IActionResult Success(String appId, Object value)
        {
            var resp = new GatewayCommonResponse()
            {
                Status = ErrorCode.SUCCESS,
                Content = value
            };

            String callResultStatus = CallResultStatus.OK.ToString();

            var signContent = JsonUtil.SerializeObject(resp.Content);
            if (!signContent.Success)
            {
                resp.Status = ErrorCode.SERIALIZE_FAILED;
                resp.Msg = ErrorCodeDescriptor.GetDescription(resp.Status);
                resp.Content = new { AppId = appId };
                return this.Json(resp, callResultStatus);
            }

            var signResult = CryptoHelper.MakeSign(signContent.Value, KeyConfig.CPICommonPrivateKey, PrivateKeyFormat.PKCS8, HashAlgorithmName.SHA1);
            if (!signResult.Success)
            {
                _logger.Error(TraceType.API.ToString(), CallResultStatus.ERROR.ToString(), $"{this.GetType().FullName}:Success()", "CryptoHelper.MakeSign(...)", "生成签名失败", signResult.FirstException);

                resp.Status = ErrorCode.SIGN_FAILED;
                resp.Msg = ErrorCodeDescriptor.GetDescription(resp.Status);
                resp.Content = new { AppId = appId };
                return this.Json(resp, callResultStatus);
            }

            resp.Sign = signResult.Value;
            return this.Json(resp, callResultStatus);
        }

        private IActionResult Json(Object value, String callResultStatus)
        {
            var result = new JsonResult(value);

            _logger.Trace(TraceType.API.ToString(), callResultStatus, $"{_typeFullName}.Json()", $"{nameof(result)}", LogPhase.ACTION, "CPI响应输出", value);

            return result;
        }
    }
}