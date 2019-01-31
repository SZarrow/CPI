using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace CPI.Common.Domain.SettleDomain.Bill99.v1_0
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class COECommonResponse
    {
        /// <summary>
        /// 
        /// </summary>
        protected COECommonResponse() { }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("requestId")]
        public String OutTradeNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("platformCode")]
        public String PlatformCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("uId")]
        public String UserId { get; set; }

        /// <summary>
        /// 快钱返回的错误码
        /// </summary>
        [JsonProperty("code")]
        public String ResponseCode { get; set; }

        /// <summary>
        /// 错误描述
        /// </summary>
        [JsonProperty("errorMsg")]
        public String ResponseMessage { get; set; }
    }
}
