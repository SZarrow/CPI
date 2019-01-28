using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace CPI.Common.Domain.SettleDomain.Bill99.v1_0
{
    /// <summary>
    /// 个人账户提现绑卡响应类
    /// </summary>
    public class PersonalWithdrawBindCardResponseV1 : CommonResponse
    {
        /// <summary>
        /// 银行卡主键Id信息，绑卡成功时返回
        /// </summary>
        [JsonProperty("memberBankAcctId")]
        public String MemberBankAccountId { get; set; }
    }
}
