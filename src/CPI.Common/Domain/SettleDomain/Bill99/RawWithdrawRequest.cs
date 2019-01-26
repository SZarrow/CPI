using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace CPI.Common.Domain.SettleDomain.Bill99
{
    /// <summary>
    /// 
    /// </summary>
    public class RawWithdrawRequest
    {
        /// <summary>
        /// 平台用户ID
        /// </summary>
        public String uId { get; set; }

        /// <summary>
        /// 外部交易号
        /// </summary>
        public String outTradeNo { get; set; }

        /// <summary>
        /// 提现金额
        /// </summary>
        [JsonConverter(typeof(AmountToCentJsonConverter))]
        public Decimal amount { get; set; }

        /// <summary>
        /// 客户自付手续费
        /// </summary>
        [JsonConverter(typeof(AmountToCentJsonConverter))]
        public Decimal customerFee { get; set; } = 0;

        /// <summary>
        /// 商户代付手续费
        /// </summary>
        [JsonConverter(typeof(AmountToCentJsonConverter))]
        public Decimal merchantFee { get; set; } = 0;
    }
}
