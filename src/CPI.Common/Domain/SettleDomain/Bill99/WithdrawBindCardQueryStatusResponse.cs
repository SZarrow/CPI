using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Domain.SettleDomain.Bill99
{
    /// <summary>
    /// 提现绑卡状态查询响应类
    /// </summary>
    public class WithdrawBindCardQueryStatusResponse : CommonResponse
    {
        /// <summary>
        /// 银行卡号
        /// </summary>
        public String BankCardNo { get; set; }
        /// <summary>
        /// 银行卡主键Id
        /// </summary>
        public String MemberBankAcctId { get; set; }
    }
}
