using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace CPI.Common.Domain.SettleDomain.Bill99
{
    /// <summary>
    /// 账户余额实体类
    /// </summary>
    public class AccountBalanceInfo
    {
        /// <summary>
        /// 账户余额类型
        /// </summary>
        public String AccountBalanceType { get; set; }

        /// <summary>
        /// 账户名
        /// </summary>
        public String AccountName { get; set; }

        /// <summary>
        /// 账户余额
        /// </summary>
        public Decimal Balance { get; set; }

        /// <summary>
        /// 账户可用余额
        /// </summary>
        public Decimal AvailableBalance { get; set; }
    }
}
