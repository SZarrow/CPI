using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace CPI.Common.Domain.SettleDomain.Bill99
{
    /// <summary>
    /// 原始个人账户提现重新绑卡响应类
    /// </summary>
    public class RawPersonalWithdrawRebindCardResponse : YZTCommonResponse
    {
        /// <summary>
        /// 银行卡主键Id信息，绑卡成功时返回
        /// </summary>
        [JsonProperty("memberBankAcctId")]
        public String MemberBankAccountId { get; set; }
    }
}
