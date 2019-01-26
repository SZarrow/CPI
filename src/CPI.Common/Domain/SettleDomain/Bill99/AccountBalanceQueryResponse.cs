using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace CPI.Common.Domain.SettleDomain.Bill99
{
    /// <summary>
    /// 账户余额查询响应类
    /// </summary>
    public class AccountBalanceQueryResponse
    {
        /// <summary>
        /// 账户余额列表
        /// </summary>
        public IEnumerable<AccountBalanceInfo> AccountBalances { get; set; }
    }
}
