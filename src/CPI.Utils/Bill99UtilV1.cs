﻿using System;
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
    public static class Bill99UtilV1
    {
        private static readonly HttpClient _client = new HttpClient();
        private static readonly ILogger _logger = LogManager.GetLogger();

        static Bill99UtilV1()
        {
            _client.DefaultRequestHeaders.Add("X-99Bill-PlatformCode", GlobalConfig.X99bill_COE_v1_PlatformCode);
        }

        public static void AddSign(HttpClient client, String signContent)
        {
            var sign = SignUtil.MakeSign(signContent, KeyConfig.Bill99_COE_v1_Hehua_PrivateKey, PrivateKeyFormat.PKCS8, "RSA");
            if (sign.Success)
            {
                client.DefaultRequestHeaders.Remove("X-99Bill-Signature");
                client.DefaultRequestHeaders.Add("X-99Bill-Signature", sign.Value);
            }
        }

        public static Boolean VerifySign(HttpResponseMessage respMsg, String respString)
        {
            if (!respMsg.Headers.TryGetValues("X-99Bill-Signature", out IEnumerable<String> respSign))
            {
                return false;
            }
            var verifyResult = SignUtil.VerifySign(respSign.FirstOrDefault(), respString, KeyConfig.Bill99_COE_v1_PublicKey, "RSA");
            return verifyResult.Success && verifyResult.Value;
        }

        public static XResult<TResponse> Execute<TRequest, TResponse>(String interfaceUrl, TRequest request)
        {
            if (request == null)
            {
                return new XResult<TResponse>(default(TResponse), new ArgumentNullException(nameof(request)));
            }

            String service = $"{typeof(Bill99UtilV1).FullName}.Execute(...)";

            var client = GetClient();

            var serializeResult = JsonUtil.SerializeObject(request);
            if (!serializeResult.Success)
            {
                return new XResult<TResponse>(default(TResponse), serializeResult.FirstException);
            }

            String postData = serializeResult.Value;
            AddSign(client, postData);

            String requestUrl = $"{ApiConfig.Bill99_COE_v1_RequestUrl}{interfaceUrl}";
            String traceMethod = $"{nameof(client)}.PostJson(...)";

            _logger.Trace(TraceType.UTIL.ToString(), CallResultStatus.OK.ToString(), service, traceMethod, LogPhase.BEGIN, "快钱COE：开始请求快钱COE接口", new Object[] { requestUrl, postData });

            var result = client.PostJson(requestUrl, postData);

            _logger.Trace(TraceType.UTIL.ToString(), (result.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), service, traceMethod, LogPhase.ACTION, "快钱COE：结束请求快钱COE接口");

            if (!result.Success)
            {
                _logger.Error(TraceType.UTIL.ToString(), CallResultStatus.ERROR.ToString(), service, traceMethod, $"快钱COE：调用快钱COE接口失败：{result.ErrorMessage}", result.FirstException);
                return new XResult<TResponse>(default(TResponse), result.FirstException);
            }

            if (result.Value == null)
            {
                _logger.Error(TraceType.UTIL.ToString(), CallResultStatus.ERROR.ToString(), service, traceMethod, $"快钱COE：调用快钱COE接口超时");
                return new XResult<TResponse>(default(TResponse), ErrorCode.REQUEST_TIMEOUT);
            }

            try
            {
                String respString = result.Value.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                _logger.Trace(TraceType.UTIL.ToString(), CallResultStatus.OK.ToString(), service, traceMethod, LogPhase.END, "快钱COE：快钱COE返回结果", respString);

                if (!VerifySign(result.Value, respString))
                {
                    _logger.Error(TraceType.UTIL.ToString(), CallResultStatus.ERROR.ToString(), service, "VerifySign(...)", "快钱COE：快钱返回的数据验签失败");
                    return new XResult<TResponse>(default(TResponse), new SignException("验签失败"));
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
