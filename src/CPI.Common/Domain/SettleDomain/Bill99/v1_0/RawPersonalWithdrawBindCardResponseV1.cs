using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace CPI.Common.Domain.SettleDomain.Bill99.v1_0
{
    /// <summary>
    /// 原始个人账户提现绑卡响应类
    /// </summary>
    public class RawPersonalWithdrawBindCardResponseV1 : COECommonResponse
    {
        /// <summary>
        /// 银行卡主键Id信息，绑卡成功时返回
        /// </summary>
        public String memberBankAcctId { get; set; }
    }
}
