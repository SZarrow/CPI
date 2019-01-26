using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Domain.SettleDomain.Bill99
{
    /// <summary>
    /// 
    /// </summary>
    public class RawAccountBalanceQueryResponse : YZTCommonResponse
    {
        /// <summary>
        /// 账户余额列表
        /// </summary>
        public IEnumerable<RawAccountBalanceInfo> accountBalanceList { get; set; }
    }
}
