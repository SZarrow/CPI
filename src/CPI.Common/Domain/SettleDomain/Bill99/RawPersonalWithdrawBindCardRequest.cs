using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Domain.SettleDomain.Bill99
{
    /// <summary>
    /// 原始个人账户提现绑卡请求类
    /// </summary>
    public class RawPersonalWithdrawBindCardRequest
    {
        /// <summary>
        /// 收款人Id
        /// </summary>
        public String uId { get; set; }

        /// <summary>
        /// 银行卡号，必填
        /// </summary>
        public String bankAcctId { get; set; }

        /// <summary>
        /// 银行预留手机号，必填
        /// </summary>
        public String mobile { get; set; }

        /// <summary>
        /// 二类账户标识，0表示非二类账户，1表示是二类账户，默认为0，可选
        /// </summary>
        public String secondAcct { get; set; } = "0";
    }
}
