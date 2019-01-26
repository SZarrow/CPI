using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace CPI.Common.Domain.SettleDomain.Bill99
{
    /// <summary>
    /// 提现手续费查询响应类
    /// </summary>
    public class RawWithdrawQueryFeeResponse : YZTCommonResponse
    {
        /// <summary>
        /// 手续费
        /// </summary>
        [JsonProperty("fee")]
        [JsonConverter(typeof(AmountFromCentJsonConverter))]
        public Decimal Fee { get; set; }
    }
}
