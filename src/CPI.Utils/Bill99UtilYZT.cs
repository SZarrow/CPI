using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using CPI.Common;
using CPI.Common.Exceptions;
using CPI.Config;
using CPI.Security;
using Lotus.Core;
using Lotus.Logging;
using Lotus.Net;
using Lotus.Security;
using Lotus.Validation;

namespace CPI.Utils
{
    public static class Bill99UtilYZT
    {
        private static readonly HttpClient _client = new HttpClient();
        private static readonly ILogger _logger = LogManager.GetLogger();

        static Bill99UtilYZT()
        {
            _client.DefaultRequestHeaders.Add("X-99Bill-PlatformCode", GlobalConfig.X99bill_YZT_PlatformCode);
        }

        public static void AddSign(HttpClient client, String signContent)
        {
            var sign = SignUtil.MakeSign(signContent, KeyConfig.Bill99YZTHehuaPrivateKey, PrivateKeyFormat.PKCS8, "RSA");
            if (sign.Success)
            {
                client.DefaultRequestHeaders.Remove("X-99Bill-Signature");
                client.DefaultRequestHeaders.Add("X-99Bill-Signature", sign.Value);
            }
        }

        public static Boolean VerifySign(HttpResponseMessage respMsg, String respString, out String errorMessage)
        {
            errorMessage = null;
            if (!respMsg.Headers.TryGetValues("X-99Bill-Signature", out IEnumerable<String> respSign))
            {
                errorMessage = "响应头中无\"X-99Bill-Signature\"字段";
                return false;
            }
            var verifyResult = SignUtil.VerifySign(respSign.FirstOrDefault(), respString, KeyConfig.Bill99YZTPublicKey, "RSA");
            if (!verifyResult.Success)
            {
                errorMessage = verifyResult.ErrorMessage;
            }
            return verifyResult.Success && verifyResult.Value;
        }

        public static XResult<TResponse> Execute<TRequest, TResponse>(String interfaceUrl, TRequest request)
        {
            if (request == null)
            {
                return new XResult<TResponse>(default(TResponse), new ArgumentNullException(nameof(request)));
            }

            String service = $"{typeof(Bill99UtilYZT).FullName}.Execute(...)";

            var client = GetClient();

            var serializeResult = JsonUtil.SerializeObject(request);
            if (!serializeResult.Success)
            {
                return new XResult<TResponse>(default(TResponse), serializeResult.FirstException);
            }

            String postData = serializeResult.Value;
            AddSign(client, postData);

            String requestUrl = $"{ApiConfig.Bill99YZTRequestUrl}{interfaceUrl}";
            String traceMethod = $"{nameof(client)}.PostJson(...)";

            _logger.Trace(TraceType.UTIL.ToString(), CallResultStatus.OK.ToString(), service, traceMethod, LogPhase.BEGIN, "快钱盈帐通：开始请求快钱盈帐通接口", new Object[] { requestUrl, postData });

            var result = client.PostJson(requestUrl, postData);

            _logger.Trace(TraceType.UTIL.ToString(), (result.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), service, traceMethod, LogPhase.ACTION, "快钱盈帐通：结束请求快钱盈帐通接口");

            if (!result.Success)
            {
                _logger.Error(TraceType.UTIL.ToString(), CallResultStatus.ERROR.ToString(), service, traceMethod, $"快钱盈帐通：调用快钱盈帐通接口失败：{result.ErrorMessage}", result.FirstException);
                return new XResult<TResponse>(default(TResponse), result.FirstException);
            }

            if (result.Value == null)
            {
                _logger.Error(TraceType.UTIL.ToString(), CallResultStatus.ERROR.ToString(), service, traceMethod, $"快钱盈帐通：调用快钱盈帐通接口超时");
                return new XResult<TResponse>(default(TResponse), ErrorCode.REQUEST_TIMEOUT);
            }

            try
            {
                String respString = result.Value.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                _logger.Trace(TraceType.UTIL.ToString(), CallResultStatus.OK.ToString(), service, traceMethod, LogPhase.END, "快钱盈帐通：快钱盈帐通返回结果", respString);

                String verifySignError;
                if (!VerifySign(result.Value, respString, out verifySignError))
                {
                    _logger.Error(TraceType.UTIL.ToString(), CallResultStatus.ERROR.ToString(), service, "VerifySign(...)", "快钱盈帐通：快钱返回的数据验签失败", new SignException(verifySignError));
                    return new XResult<TResponse>(default(TResponse), new SignException("快钱返回的数据验签失败"));
                }

                return JsonUtil.DeserializeObject<TResponse>(respString);
            }
            catch (Exception ex)
            {
                return new XResult<TResponse>(default(TResponse), ex);
            }
        }

        private static HttpClient GetClient()
        {
            var client = _client;
            client.DefaultRequestHeaders.Remove("X-99Bill-TraceId");
            String traceId = _logger.CurrentTraceId;
            client.DefaultRequestHeaders.Add("X-99Bill-TraceId", traceId);
            return client;
        }
    }
}
