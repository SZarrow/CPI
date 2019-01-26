using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace CPI.Common.Domain.SettleDomain.Bill99
{
    /// <summary>
    /// 提现绑卡的银行卡信息
    /// </summary>
    public class WithdrawBindCardInfo
    {
        /// <summary>
        /// 银行卡主键Id
        /// </summary>
        [JsonProperty("memberBankAcctId")]
        public String MemberBankAcctId { get; set; }

        /// <summary>
        /// 银行卡号
        /// </summary>
        [JsonProperty("bankAcctId")]
        public String BankCardNo { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        [JsonProperty("mobile")]
        public String Mobile { get; set; }

        /// <summary>
        /// 持卡人姓名
        /// </summary>
        [JsonProperty("name")]
        public String CardHolder { get; set; }

        /// <summary>
        /// 银行编号
        /// </summary>
        [JsonProperty("bankId")]
        public String BankNo { get; set; }

        /// <summary>
        /// 银行编码
        /// </summary>
        [JsonProperty("bankCode")]
        public String BankCode { get; set; }

        /// <summary>
        /// 账户类型，默认0，出入金。
        /// </summary>
        [JsonProperty("accountType")]
        public String AccountType { get; set; }

        /// <summary>
        /// 是否是主卡，1表示主卡，0表示非主卡
        /// </summary>
        [JsonProperty("primaryBankAcct")]
        public String IsPrimaryCard { get; set; }

        /// <summary>
        /// 银行名称
        /// </summary>
        [JsonProperty("bankName")]
        public String BankName { get; set; }

        /// <summary>
        /// 银行卡状态，0：未验，1：已验证，2：验证中，9：注销 
        /// </summary>
        [JsonProperty("status")]
        public String Status { get; set; }
    }
}
