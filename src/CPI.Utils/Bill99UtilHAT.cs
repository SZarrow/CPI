﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using CPI.Common;
using CPI.Common.Exceptions;
using CPI.Config;
using CPI.Security;
using ATBase.Core;
using ATBase.Logging;
using ATBase.Net;
using ATBase.Security;
using ATBase.Validation;

namespace CPI.Utils
{
    public static class Bill99UtilHAT
    {
        private static readonly ILogger _logger = LogManager.GetLogger();
        private static readonly IHttpClientFactory _httpClientFactory = XDI.Resolve<IHttpClientFactory>();

        public static String AddSign(HttpClient client, String signContent)
        {
            var sign = SignUtil.MakeSign(signContent, KeyConfig.Bill99_HAT_Hehua_PrivateKey, PrivateKeyFormat.PKCS8, "RSA");
            if (sign.Success)
            {
                client.DefaultRequestHeaders.Remove("X-99Bill-Signature");
                client.DefaultRequestHeaders.Add("X-99Bill-Signature", sign.Value);
                return sign.Value;
            }

            return "生成签名失败";
        }

        public static Boolean VerifySign(HttpResponseMessage respMsg, String respString, out String errorMessage)
        {
            errorMessage = null;
            if (!respMsg.Headers.TryGetValues("X-99Bill-Signature", out IEnumerable<String> respSign))
            {
                errorMessage = "响应头中无\"X-99Bill-Signature\"字段";
                return false;
            }

            String signString = respSign.FirstOrDefault();
            if (signString.IsNullOrWhiteSpace())
            {
                errorMessage = "响应头中\"X-99Bill-Signature\"字段的值为空";
                return false;
            }

            var verifyResult = SignUtil.VerifySign(signString, respString, KeyConfig.Bill99_HAT_PublicKey, "RSA");
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

            String service = $"{typeof(Bill99UtilHAT).FullName}.Execute(...)";

            var client = GetClient();

            var serializeResult = JsonUtil.SerializeObject(request);
            if (!serializeResult.Success)
            {
                return new XResult<TResponse>(default(TResponse), serializeResult.FirstException);
            }

            String postData = serializeResult.Value;
            String sign = AddSign(client, postData);

            String requestUrl = $"{ApiConfig.Bill99_HAT_RequestUrl}{interfaceUrl}";
            String traceMethod = $"{nameof(client)}.PostJson(...)";

            _logger.Trace(TraceType.UTIL.ToString(), CallResultStatus.OK.ToString(), service, traceMethod, LogPhase.BEGIN, "快钱HAT：开始请求快钱HAT接口", new Object[] { requestUrl, postData, $"X-99Bill-Signature：{sign}" });

            var result = client.PostJson(requestUrl, postData);

            _logger.Trace(TraceType.UTIL.ToString(), (result.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), service, traceMethod, LogPhase.ACTION, "快钱HAT：结束请求快钱HAT接口");

            if (!result.Success)
            {
                _logger.Error(TraceType.UTIL.ToString(), CallResultStatus.ERROR.ToString(), service, traceMethod, $"快钱HAT：调用快钱HAT接口失败：{result.ErrorMessage}", result.FirstException);
                return new XResult<TResponse>(default(TResponse), result.FirstException);
            }

            if (result.Value == null)
            {
                _logger.Error(TraceType.UTIL.ToString(), CallResultStatus.ERROR.ToString(), service, traceMethod, $"快钱HAT：调用快钱HAT接口超时");
                return new XResult<TResponse>(default(TResponse), ErrorCode.REQUEST_TIMEOUT);
            }

            try
            {
                String respString = result.Value.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                _logger.Trace(TraceType.UTIL.ToString(), CallResultStatus.OK.ToString(), service, traceMethod, LogPhase.END, "快钱HAT：快钱HAT返回结果", respString);

                String verifySignError = null;
                if (!VerifySign(result.Value, respString, out verifySignError))
                {
                    _logger.Error(TraceType.UTIL.ToString(), CallResultStatus.ERROR.ToString(), service, "VerifySign(...)", "快钱HAT：快钱返回的数据验签失败", new SignException(verifySignError));
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
            var client = _httpClientFactory.CreateClient("CommonHttpClient");
            client.DefaultRequestHeaders.Remove("X-99Bill-PlatformCode");
            client.DefaultRequestHeaders.Add("X-99Bill-PlatformCode", GlobalConfig.X99bill_HAT_PlatformCode);
            client.DefaultRequestHeaders.Remove("X-99Bill-TraceId");
            String traceId = _logger.CurrentTraceId;
            client.DefaultRequestHeaders.Add("X-99Bill-TraceId", traceId);
            return client;
        }
    }
}
