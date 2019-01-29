using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Domain.SettleDomain.Bill99.v1_0
{
    /// <summary>
    /// 
    /// </summary>
    public class RawQueryBankCardAcceptRequestV1
    {
        /// <summary>
        /// 
        /// </summary>
        public String requestId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public String platformCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public String uId { get; set; }
        /// <summary>
        /// 银行卡号
        /// </summary>
        public String bankAcctId { get; set; }
    }
}
