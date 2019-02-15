using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace CPI.Common.Domain.SettleDomain.Bill99
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class YZTCommonResponse
    {
        /// <summary>
        /// 
        /// </summary>
        protected YZTCommonResponse() { }

        /// <summary>
        /// 0000为调用成功
        /// </summary>
        [JsonProperty("rspCode")]
        public String ResponseCode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("rspMsg")]
        public String ResponseMessage { get; set; }
    }
}
