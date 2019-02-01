using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Domain.SettleDomain.Bill99.v1_0
{
    /// <summary>
    /// 
    /// </summary>
    public class QueryBankCardAcceptResponseV1 : CommonResponse
    {
        /// <summary>
        /// 
        /// </summary>
        public String UserId { get; set; }
        /// <summary>
        /// 卡类型
        /// </summary>
        public String CardType { get; set; }
        /// <summary>
        /// 银行编号
        /// </summary>
        public String BankCode { get; set; }
        /// <summary>
        /// 银行名称
        /// </summary>
        public String BankName { get; set; }
    }
}
