using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace CPI.Common.Domain.SettleDomain.Bill99
{
    /// <summary>
    /// 
    /// </summary>
    public class RawPersonalBoundCardListQueryRequest
    {
        /// <summary>
        /// 用户ID 
        /// </summary>
        [JsonProperty("uId")]
        public String PayeeId { get; set; }
    }
}
