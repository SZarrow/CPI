using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Domain.SettleDomain.Bill99
{
    /// <summary>
    /// 
    /// </summary>
    public class RawPersonalBindCardStatusQueryRequest
    {
        /// <summary>
        /// 平台用户Id
        /// </summary>
        public String uId { get; set; }
        /// <summary>
        /// 银行卡号
        /// </summary>
        public String bankAcctId { get; set; }
    }
}
