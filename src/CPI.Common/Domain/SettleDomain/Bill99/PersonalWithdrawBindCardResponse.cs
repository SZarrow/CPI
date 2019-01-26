using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace CPI.Common.Domain.SettleDomain.Bill99
{
    /// <summary>
    /// 个人账户提现绑卡响应类
    /// </summary>
    public class PersonalWithdrawBindCardResponse : CommonResponse
    {
        /// <summary>
        /// 银行卡主键Id信息，绑卡成功时返回
        /// </summary>
        [JsonProperty("memberBankAcctId")]
        public String MemberBankAccountId { get; set; }
    }
}
