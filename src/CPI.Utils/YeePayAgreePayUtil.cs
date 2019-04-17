using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web;
using CPI.Common;
using CPI.Common.Domain.FundOut.YeePay;
using CPI.Common.Exceptions;
using CPI.Config;
using CPI.Security;
using ATBase.Core;
using ATBase.Logging;
using ATBase.Net;
using ATBase.Security;
using Microsoft.Extensions.Configuration;

namespace CPI.Utils
{
    /// <summary>
    /// 易宝代付助手类
    /// </summary>
    public static class YeePayAgreePayUtil
    {
        private static readonly ILogger _logger = LogManager.GetLogger();
        private static readonly IHttpClientFactory _httpClientFactory = XDI.Resolve<IHttpClientFactory>();

        private static void AddSign(HttpClient client, String interfaceUrl, String requestBody)
        {
            String requestId = Guid.NewGuid().ToString("N");
            String timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzzz");
            String version = "yop-auth-v2";
            String expireSeconds = "1800";
            String appKey = GlobalConfig.YeePay_AgreePay_AppKey;

            var signHeaders = new SortedDictionary<String, String>();
            signHeaders["x-yop-appkey"] = appKey;
            signHeaders["x-yop-request-id"] = requestId;
            signHeaders["x-yop-date"] = timestamp;

            foreach (var key in signHeaders.Keys)
            {
                client.DefaultRequestHeaders.Remove(key);
                client.DefaultRequestHeaders.Add(key, signHeaders[key]);
            }

            //签名内容的请求头部分
            String signHeaderSignContent = String.Join("\n", signHeaders.Select(x => $"{WebUtil.UrlEncode(x.Key)}:{UrlEncodeToUpper(WebUtil.UrlEncode(x.Value))}"));

            //签名内容
            String signContent = $"{version}/{appKey}/{timestamp}/{expireSeconds}\nPOST\n{interfaceUrl}\n{requestBody}\n{signHeaderSignContent}";

            var sign = SignUtil.MakeSign(signContent, KeyConfig.YeePay_AgreePay_Hehua_PrivateKey, PrivateKeyFormat.PKCS1, "RSA2");
            if (sign.Success)
            {
                String signHeaderNames = String.Join(";", signHeaders.Keys);
                String base64SignContent = EncodeBase64(sign.Value) + "$SHA256";

                client.DefaultRequestHeaders.Remove("Authorization");
                String auth = GetAuthorization(version, appKey, timestamp, expireSeconds, signHeaderNames, base64SignContent);
                _logger.Debug($"Authorization={auth}");
                client.DefaultRequestHeaders.Add("Authorization", auth);
            }
        }

        private static String GetTimeStamp()
        {
            const Int64 TicksOf1970 = 621355968000000000;
            return ((DateTime.Now.ToUniversalTime().Ticks - TicksOf1970) / 10000000L).ToString();
        }

        private static String UrlEncodeToUpper(String value)
        {
            if (value.HasValue())
            {
                return Regex.Replace(value, @"%[a-f0-9]{2}", m => m.Value.ToUpperInvariant());
            }

            return value;
        }

        private static String EncodeBase64(String content)
        {
            if (content.IsNullOrWhiteSpace())
            {
                return content;
            }

            Int32 splitIndex = content.IndexOf('=');
            if (splitIndex > 0)
            {
                content = content.Substring(0, splitIndex);
            }

            return content.Replace('+', '-').Replace('/', '_');
        }

        private static String DecodeBase64(String base64String)
        {
            if (base64String.IsNullOrWhiteSpace())
            {
                return base64String;
            }

            base64String = base64String.Replace('-', '+').Replace('_', '/');

            switch (base64String.Length % 4)
            {
                case 0: break;
                case 2: base64String += "=="; break;
                case 3: base64String += "="; break;
                default:
                    throw new ArgumentException("无效的Base64字符串");
            }

            return base64String;
        }

        private static String GetAuthorization(String version, String appKey, String timestamp, String expireSeconds, String signHeaderNames, String base64SignContent)
        {
            return $"YOP-RSA2048-SHA256 {version}/{appKey}/{timestamp}/{expireSeconds}/{signHeaderNames}/{base64SignContent}";
        }

        private static Boolean VerifySign(String signContent, String sign, out String errorMessage)
        {
            errorMessage = null;

            sign = DecodeBase64(sign.Substring(0, sign.Length - "$SHA256".Length));

            var verifyResult = SignUtil.VerifySign(sign, signContent, KeyConfig.YeePay_AgreePay_PublicKey, "RSA2");
            if (!verifyResult.Success)
            {
                errorMessage = verifyResult.ErrorMessage;
            }
            return verifyResult.Success && verifyResult.Value;
        }

        public static XResult<TResult> Execute<TRequest, TResult>(String interfaceUrl, TRequest request)
        {
            if (request == null)
            {
                return new XResult<TResult>(default(TResult), new ArgumentNullException(nameof(request)));
            }

            String service = $"{typeof(YeePayAgreePayUtil).FullName}.{nameof(Execute)}(...)";

            var client = GetClient();

            var requestDic = CommonUtil.ToDictionary(request);
            if (requestDic == null || requestDic.Count == 0)
            {
                return new XResult<TResult>(default(TResult), ErrorCode.INVALID_CAST, new InvalidCastException("将请求对象转换成字典失败"));
            }

            //签名内容的请求内容部分
            requestDic["method"] = interfaceUrl;
            requestDic["appKey"] = GlobalConfig.YeePay_AgreePay_AppKey;
            requestDic["locale"] = "zh_CN";
            requestDic["ts"] = GetTimeStamp();
            requestDic["v"] = "1.0";

            var orderedDic = new SortedDictionary<String, String>(requestDic);
            foreach (var key in orderedDic.Keys.ToList())
            {
                orderedDic[key] = UrlEncodeToUpper(WebUtil.UrlEncode(orderedDic[key]));
            }

            String requestBody = String.Join("&",
                     from t0 in orderedDic
                     select $"{t0.Key}={t0.Value}");

            AddSign(client, interfaceUrl, requestBody);

            String requestUrl = $"{ApiConfig.YeePay_AgreePay_RequestUrl}{interfaceUrl}";
            String traceMethod = $"{nameof(client)}.PostForm(...)";

            _logger.Trace(TraceType.UTIL.ToString(), CallResultStatus.OK.ToString(), service, traceMethod, LogPhase.BEGIN, "开始请求易宝协议支付接口", new Object[] { requestUrl, requestDic });

            var result = client.PostForm(requestUrl, orderedDic);

            _logger.Trace(TraceType.UTIL.ToString(), (result.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), service, traceMethod, LogPhase.ACTION, "结束请求易宝协议支付接口");

            if (!result.Success)
            {
                _logger.Error(TraceType.UTIL.ToString(), CallResultStatus.ERROR.ToString(), service, traceMethod, $"调用易宝协议支付接口失败：{result.ErrorMessage}", result.FirstException);
                return new XResult<TResult>(default(TResult), result.FirstException);
            }

            if (result.Value == null)
            {
                _logger.Error(TraceType.UTIL.ToString(), CallResultStatus.ERROR.ToString(), service, traceMethod, $"调用易宝协议支付接口超时");
                return new XResult<TResult>(default(TResult), ErrorCode.REQUEST_TIMEOUT);
            }

            try
            {
                String respString = result.Value.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                String compressedRespString = CompressJsonString(respString);

                _logger.Trace(TraceType.UTIL.ToString(), CallResultStatus.OK.ToString(), service, traceMethod, LogPhase.END, "易宝协议支付返回结果", compressedRespString);

                if (respString.IsNullOrWhiteSpace())
                {
                    return new XResult<TResult>(default(TResult), ErrorCode.REMOTE_RETURN_NOTHING, new RemoteException("支付机构未返回任何数据"));
                }

                var respDic = JsonUtil.GetJToken(compressedRespString);

                var state = respDic.GetValue<String>("state");

                if (state == "FAILURE")
                {
                    var errorDecodeResult = JsonUtil.DeserializeObject<IDictionary<String, String>>(respDic["error"].ToString());
                    if (!errorDecodeResult.Success)
                    {
                        _logger.Error(TraceType.UTIL.ToString(), CallResultStatus.ERROR.ToString(), service, "errorDecodeResult", "易宝返回的数据无法反序列化", errorDecodeResult.FirstException, respDic["error"]);
                        return new XResult<TResult>(default(TResult), ErrorCode.DESERIALIZE_FAILED, errorDecodeResult.FirstException);
                    }

                    return new XResult<TResult>(default(TResult), ErrorCode.FAILURE, new RemoteException(errorDecodeResult.Value["message"]));
                }

                //验签返回的结果
                String signContent = GetResultJsonValue(compressedRespString);
                if (!VerifySign(signContent, respDic.GetValue<String>("sign"), out String signError))
                {
                    return new XResult<TResult>(default(TResult), ErrorCode.SIGN_VERIFY_FAILED, new SignException(signError));
                }

                var payResult = JsonUtil.DeserializeObject<TResult>(signContent);
                if (!payResult.Success)
                {
                    _logger.Error(TraceType.UTIL.ToString(), CallResultStatus.ERROR.ToString(), service, "payResult", "易宝返回的数据无法反序列化", payResult.FirstException, respDic["error"]);
                    return new XResult<TResult>(default(TResult), ErrorCode.DESERIALIZE_FAILED, new ArgumentException($"参数payResult无法转换成{nameof(TResult)}类型"));
                }

                return new XResult<TResult>(payResult.Value);
            }
            catch (Exception ex)
            {
                return new XResult<TResult>(default(TResult), ex);
            }
        }

        /// <summary>
        /// 获取用于签名的内容
        /// </summary>
        /// <param name="compressedJsonString">压缩的Json字符串</param>
        private static String GetResultJsonValue(String compressedJsonString)
        {
            String startStr = "\"result\":";
            Int32 startIndex = compressedJsonString.IndexOf(startStr) + startStr.Length;
            Int32 endIndex = compressedJsonString.IndexOf(",\"ts\":");
            Int32 length = endIndex > startIndex ? endIndex - startIndex : 0;
            return compressedJsonString.Substring(startIndex, length).Trim();
        }

        private static String CompressJsonString(String respString)
        {
            return respString.Replace("\t", String.Empty).Replace("\n", String.Empty).Replace(" ", String.Empty);
        }

        private static HttpClient GetClient()
        {
            var client = _httpClientFactory.CreateClient("CommonHttpClient");
            return client;
        }
    }
}
