using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace CPI.Common.Domain.SettleDomain.Bill99
{
    /// <summary>
    /// 
    /// </summary>
    public class RawPersonalBoundCardListQueryResponse : YZTCommonResponse
    {
        /// <summary>
        /// 提现绑卡列表
        /// </summary>
        [JsonProperty("bindCardList")]
        public IEnumerable<WithdrawBindCardInfo> BindCards { get; set; }
    }
}
