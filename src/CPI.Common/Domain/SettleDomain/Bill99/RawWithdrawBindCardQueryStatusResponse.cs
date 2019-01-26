using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace CPI.Common.Domain.SettleDomain.Bill99
{
    /// <summary>
    /// 提现绑卡状态查询响应类
    /// </summary>
    public class RawWithdrawBindCardQueryStatusResponse : YZTCommonResponse
    {
        /// <summary>
        /// 绑卡状态列表
        /// </summary>
        [JsonProperty("bindCardList")]
        public IEnumerable<WithdrawBindCardInfo> BindCards { get; set; }
    }
}
