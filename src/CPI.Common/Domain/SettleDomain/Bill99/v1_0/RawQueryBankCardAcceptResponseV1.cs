using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Domain.SettleDomain.Bill99.v1_0
{
    /// <summary>
    /// 
    /// </summary>
    public class RawQueryBankCardAcceptResponseV1 : COECommonResponse
    {
        /// <summary>
        /// 卡类型
        /// </summary>
        public String cardType { get; set; }
        /// <summary>
        /// 银行编号
        /// </summary>
        public String bankId { get; set; }
        /// <summary>
        /// 银行名称
        /// </summary>
        public String bankName { get; set; }
    }
}
