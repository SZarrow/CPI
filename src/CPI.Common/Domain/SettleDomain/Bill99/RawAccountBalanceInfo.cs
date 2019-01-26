using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace CPI.Common.Domain.SettleDomain.Bill99
{
    /// <summary>
    /// 
    /// </summary>
    public class RawAccountBalanceInfo
    {
        /// <summary>
        /// 账户余额类型
        /// </summary>
        public String accountBalanceType { get; set; }

        /// <summary>
        /// 账户名
        /// </summary>
        public String accountName { get; set; }

        /// <summary>
        /// 账户余额
        /// </summary>
        [JsonConverter(typeof(AmountFromCentJsonConverter))]
        public Decimal balance { get; set; }

        /// <summary>
        /// 账户可用余额
        /// </summary>
        [JsonConverter(typeof(AmountFromCentJsonConverter))]
        public Decimal availableBalance { get; set; }
    }
}
