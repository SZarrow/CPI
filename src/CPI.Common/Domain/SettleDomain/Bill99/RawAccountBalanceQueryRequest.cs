using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Domain.SettleDomain.Bill99
{
    /// <summary>
    /// 
    /// </summary>
    public class RawAccountBalanceQueryRequest
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public String uId { get; set; }

        /// <summary>
        /// 账户余额类型
        /// </summary>
        public IEnumerable<String> accountBalanceType { get; set; }
    }
}
