using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using ATBase.Validation;
using Newtonsoft.Json;

namespace CPI.Common.Domain.SettleDomain.Bill99
{
    /// <summary>
    /// 
    /// </summary>
    public class RawPersonalInfoQueryRequest
    {
        /// <summary>
        /// 收款人Id
        /// </summary>
        [JsonProperty("uId")]
        public String PayeeId { get; set; }
    }
}
