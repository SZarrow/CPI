using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text;
using Lotus.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace CPI.Common
{
    /// <summary>
    /// 通用请求实体类
    /// </summary>
    [Serializable]
    public sealed class GatewayCommonRequest : ValidateModel
    {
        private String _method = null;

        /// <summary>
        /// 
        /// </summary>
        public GatewayCommonRequest() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="collection"></param>
        public GatewayCommonRequest(IFormCollection collection)
        {
            if (collection != null)
            {
                if (collection.TryGetValue(nameof(AppId), out StringValues appid))
                {
                    this.AppId = appid;
                }

                if (collection.TryGetValue(nameof(Method), out StringValues method))
                {
                    this.Method = method;
                }

                if (collection.TryGetValue(nameof(Version), out StringValues version))
                {
                    this.Version = version;
                }

                if (collection.TryGetValue(nameof(Timestamp), out StringValues timestamp))
                {
                    this.Timestamp = timestamp;
                }

                if (collection.TryGetValue(nameof(BizContent), out StringValues bizContent))
                {
                    this.BizContent = bizContent;
                }

                if (collection.TryGetValue(nameof(SignType), out StringValues signType))
                {
                    this.SignType = signType;
                }

                if (collection.TryGetValue(nameof(Sign), out StringValues sign))
                {
                    this.Sign = sign;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        public GatewayCommonRequest(Stream stream)
        {
            using (var sr = new StreamReader(stream, Encoding.UTF8))
            {
                try
                {
                    var req = JsonConvert.DeserializeObject<GatewayCommonRequest>(sr.ReadToEnd());
                    this.AppId = req.AppId;
                    this.Method = req.Method;
                    this.Version = req.Version;
                    this.Timestamp = req.Timestamp;
                    this.BizContent = req.BizContent;
                    this.SignType = req.SignType;
                    this.Sign = req.Sign;
                }
                catch { }
            }
        }

        /// <summary>
        /// 应用ID
        /// </summary>
        [Required(ErrorMessage = "参数AppId必需")]
        public String AppId { get; set; }
        /// <summary>
        /// 接口名称
        /// </summary>
        [Required(ErrorMessage = "参数Method必需")]
        public String Method
        {
            get
            {
                return _method;
            }
            set
            {
                _method = value != null ? value.ToLowerInvariant() : value;
            }
        }
        /// <summary>
        /// 调用的接口版本，固定为：1.0
        /// </summary>
        [Required(ErrorMessage = "参数Version必需")]
        public String Version { get; set; }
        /// <summary>
        /// 发送请求的时间，格式"yyyy-MM-dd HH:mm:ss"
        /// </summary>
        [Required(ErrorMessage = "参数Timestamp必需")]
        [RegularExpression(@"^\d{4}\-\d{2}\-\d{2}\s\d{2}:\d{2}:\d{2}$", ErrorMessage = "参数Timestamp格式错误")]
        public String Timestamp { get; set; }
        /// <summary>
        /// 请求参数集合的Json字符串，最大长度不限，除公共参数外所有请求参数都必须放在这个参数中传递，具体参照各产品快速接入文档
        /// </summary>
        [Required(ErrorMessage = "参数BizContent必需")]
        public String BizContent { get; set; }
        /// <summary>
        /// 商户生成签名字符串所使用的签名算法类型，目前支持RSA2和RSA，推荐使用RSA2
        /// </summary>
        [Required(ErrorMessage = "参数SignType必需")]
        public String SignType { get; set; }
        /// <summary>
        /// 商户请求参数的签名字符串
        /// </summary>
        [Required(ErrorMessage = "参数Sign必需")]
        public String Sign { get; set; }
    }
}
