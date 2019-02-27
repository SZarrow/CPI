using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using CPI.Common;
using CPI.Common.Exceptions;
using CPI.Config;
using CPI.Providers;
using CPI.Security;
using Lotus.Core;
using Lotus.Logging;
using Lotus.Net;
using Lotus.Security;

namespace CPI.Utils
{
    /// <summary>
    /// 易宝代付助手类
    /// </summary>
    public static class YeePayFundOutUtil
    {
        private static readonly ILogger _logger = LogManager.GetLogger();
        private static readonly IHttpClientFactory _httpClientFactory = XDI.Resolve<IHttpClientFactory>();

        public static void AddSign(HttpClient client, IDictionary<String, String> formValues)
        {
            String requestId = Guid.NewGuid().ToString("N");
            String timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzzz");
            String version = "yop-auth-v2";
            String expireSeconds = "1800";
            String appKey = GlobalConfig.YeePay_FundOut_AppKey;

            var signHeaders = new Dictionary<String, String>(3);
            signHeaders["x-yop-request-id"] = requestId;
            signHeaders["x-yop-date"] = timestamp;
            signHeaders["x-yop-appkey"] = appKey;

            foreach (var key in signHeaders.Keys)
            {
                client.DefaultRequestHeaders.Remove(key);
                client.DefaultRequestHeaders.Add(key, signHeaders[key]);
            }

            //签名内容的请求地址部分
            String requestPath = ApiConfig.YeePay_FundOut_Pay_RequestUrl.Substring(ApiConfig.YeePay_FundOut_Pay_RequestUrl.IndexOf("/rest/"));

            //签名内容的请求内容部分
            var content = new FormUrlEncodedContent(new SortedDictionary<String, String>(formValues));
            String requestBody = content.ReadAsStringAsync().GetAwaiter().GetResult();

            //签名内容的请求头部分
            String signHeaderSignContent = String.Join("\n", signHeaders.Select(x => $"{HttpUtility.UrlEncode(x.Key)}:{HttpUtility.UrlEncode(x.Value)}"));

            //签名内容
            String signContent = $@"{version}/{appKey}/{timestamp}/{expireSeconds}\nPOST\n{requestPath}\n{requestBody}\n{signHeaderSignContent}";

            var sign = SignUtil.MakeSign(signContent, KeyConfig.YeePay_FundOut_Hehua_PrivateKey, PrivateKeyFormat.PKCS1, "RSA");
            if (sign.Success)
            {
                String signHeaderNames = String.Join(";", signHeaders.Keys);
                String base64SignContent = EncodeBase64(sign.Value);

                client.DefaultRequestHeaders.Remove("Authorization");
                client.DefaultRequestHeaders.Add("Authorization", GetAuthorization(version, appKey, timestamp, expireSeconds, signHeaderNames, base64SignContent));
            }
        }

        public static String EncodeBase64(String content)
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="base64String"></param>
        /// <exception cref="ArgumentException"></exception>
        public static String DecodeBase64(String base64String)
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

            var verifyResult = SignUtil.VerifySign(signString, respString, KeyConfig.Bill99YZTPublicKey, "RSA");
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

            var requestDic = CommonUtil.ToDictionary(request);
            if (requestDic == null || requestDic.Count == 0)
            {
                return new XResult<TResponse>(default(TResponse), ErrorCode.INVALID_CAST, new InvalidCastException("将请求对象转换成字典失败"));
            }

            AddSign(client, requestDic);

            String requestUrl = $"{ApiConfig.Bill99YZTRequestUrl}{interfaceUrl}";
            String traceMethod = $"{nameof(client)}.PostForm(...)";

            _logger.Trace(TraceType.UTIL.ToString(), CallResultStatus.OK.ToString(), service, traceMethod, LogPhase.BEGIN, "开始请求易宝代付接口", new Object[] { requestUrl, requestDic });

            var result = client.PostForm(requestUrl, requestDic);

            _logger.Trace(TraceType.UTIL.ToString(), (result.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), service, traceMethod, LogPhase.ACTION, "结束请求易宝代付接口");

            if (!result.Success)
            {
                _logger.Error(TraceType.UTIL.ToString(), CallResultStatus.ERROR.ToString(), service, traceMethod, $"调用易宝代付接口失败：{result.ErrorMessage}", result.FirstException);
                return new XResult<TResponse>(default(TResponse), result.FirstException);
            }

            if (result.Value == null)
            {
                _logger.Error(TraceType.UTIL.ToString(), CallResultStatus.ERROR.ToString(), service, traceMethod, $"调用易宝代付接口超时");
                return new XResult<TResponse>(default(TResponse), ErrorCode.REQUEST_TIMEOUT);
            }

            try
            {
                String respString = result.Value.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                _logger.Trace(TraceType.UTIL.ToString(), CallResultStatus.OK.ToString(), service, traceMethod, LogPhase.END, "易宝代付返回结果", respString);

                String verifySignError;
                if (!VerifySign(result.Value, respString, out verifySignError))
                {
                    _logger.Error(TraceType.UTIL.ToString(), CallResultStatus.ERROR.ToString(), service, "VerifySign(...)", "易宝代付返回的数据验签失败", new SignException(verifySignError));
                    return new XResult<TResponse>(default(TResponse), new SignException("易宝代付返回的数据验签失败"));
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
            return client;
        }
    }
}
