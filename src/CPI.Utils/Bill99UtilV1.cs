using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using CPI.Common;
using CPI.Common.Exceptions;
using CPI.Config;
using CPI.Security;
using Lotus.Core;
using Lotus.Logging;
using Lotus.Net;
using Lotus.Security;

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

            _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.OK.ToString(), service, "postData", LogPhase.ACTION, "请求消息体明文", postData);

            var encryptedResult = Encrypt(postData, CryptoHelper.GenerateRandomKey());
            if (!encryptedResult.Success)
            {
                return new XResult<TResponse>(default(TResponse), ErrorCode.ENCRYPT_FAILED, encryptedResult.FirstException);
            }

            String requestUrl = $"{ApiConfig.Bill99_COE_v1_RequestUrl}{interfaceUrl}";
            String traceMethod = $"{nameof(client)}.PostJson(...)";

            _logger.Trace(TraceType.UTIL.ToString(), CallResultStatus.OK.ToString(), service, traceMethod, LogPhase.BEGIN, "快钱COE：开始请求快钱COE接口", new Object[] { requestUrl, encryptedResult.Value });

            var result = client.PostJson(requestUrl, encryptedResult.Value);

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

                var decodeResponseResult = JsonUtil.DeserializeObject<Dictionary<String, String>>(respString);

                if (!decodeResponseResult.Success)
                {
                    return new XResult<TResponse>(default(TResponse), ErrorCode.DESERIALIZE_FAILED, decodeResponseResult.FirstException);
                }

                var respDic = decodeResponseResult.Value;

                String envelope = respDic["envelope"];

                if (envelope.IsNullOrWhiteSpace())
                {
                    return new XResult<TResponse>(default(TResponse), ErrorCode.INFO_NOT_EXIST, new ArgumentException($"快钱未返回{nameof(envelope)}字段"));
                }

                Byte[] digitalEnvData = null;
                try
                {
                    digitalEnvData = Convert.FromBase64String(envelope);
                }
                catch (Exception ex)
                {
                    return new XResult<TResponse>(default(TResponse), ErrorCode.DECODE_FAILED, ex);
                }

                Byte[] key = null;
                using (var ms = new MemoryStream(digitalEnvData))
                {
                    var decryptKeyResult = CryptoHelper.RSADecrypt(ms, KeyConfig.Bill99_COE_v1_Hehua_PrivateKey, PrivateKeyFormat.PKCS8);
                    if (!decryptKeyResult.Success)
                    {
                        return new XResult<TResponse>(default(TResponse), ErrorCode.DECRYPT_FAILED, decryptKeyResult.FirstException);
                    }

                    key = decryptKeyResult.Value;
                }

                String encryptedBase64String = respDic["encryptedData"];

                Byte[] encryptedData = null;
                try
                {
                    encryptedData = Convert.FromBase64String(encryptedBase64String);
                }
                catch (Exception ex)
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, "Convert.FromBase64String(...)", "encryptedData不是有效的Base64字符串");
                    return new XResult<TResponse>(default(TResponse), ErrorCode.DECODE_FAILED, ex);
                }

                var decryptedResult = CryptoHelper.AESDecrypt(encryptedData, key);
                if (!decryptedResult.Success)
                {
                    return new XResult<TResponse>(default(TResponse), ErrorCode.DECRYPT_FAILED, decryptedResult.FirstException);
                }

                String signBase64String = respDic["signature"];

                Byte[] sign = null;
                try
                {
                    sign = Convert.FromBase64String(signBase64String);
                }
                catch (Exception ex)
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, "Convert.FromBase64String(...)", "signedData不是有效的Base64字符串", ex);
                    return new XResult<TResponse>(default(TResponse), ErrorCode.DECODE_FAILED, new RemoteException("signedData不是有效的Base64字符串"));
                }

                Byte[] signContent = decryptedResult.Value;

                var verifyResult = CryptoHelper.VerifySign(sign, signContent, KeyConfig.Bill99_COE_v1_PublicKey, HashAlgorithmName.SHA1);
                if (!verifyResult.Value)
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, "verifyResult", "验签失败", verifyResult.FirstException);
                    return new XResult<TResponse>(default(TResponse), ErrorCode.SIGN_VERIFY_FAILED, new SignException("sign verify failed"));
                }

                try
                {
                    String decryptedValue = Encoding.UTF8.GetString(decryptedResult.Value);
                    _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.OK.ToString(), service, nameof(decryptedValue), LogPhase.ACTION, "解密得到结果", decryptedValue);
                    return JsonUtil.DeserializeObject<TResponse>(decryptedValue);
                }
                catch (Exception ex)
                {
                    return new XResult<TResponse>(default(TResponse), ErrorCode.DECODE_FAILED, ex);
                }
            }
            catch (Exception ex)
            {
                return new XResult<TResponse>(default(TResponse), ex);
            }
        }

        private static XResult<String> Encrypt(String postData, Byte[] key)
        {
            var data = Encoding.UTF8.GetBytes(postData);
            var encryptedResult = CryptoHelper.AESEncrypt(data, key);
            if (!encryptedResult.Success)
            {
                _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), "Encrypt(...)", "encryptedResult", "生成密文失败", encryptedResult.FirstException, postData);
                return new XResult<String>(null, ErrorCode.ENCRYPT_FAILED, encryptedResult.FirstException);
            }

            try
            {
                String base64String = Convert.ToBase64String(encryptedResult.Value);
                return new XResult<String>(base64String);
            }
            catch (Exception ex)
            {
                return new XResult<String>(null, ErrorCode.ENCODE_FAILED, ex);
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
