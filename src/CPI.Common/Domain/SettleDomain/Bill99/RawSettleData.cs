using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace CPI.Common.Domain.SettleDomain.Bill99
{
    /// <summary>
    /// 
    /// </summary>
    public class RawSettleData
    {
        /// <summary>
        /// 收款账户Id
        /// </summary>
        public String merchantUid { get; set; }

        /// <summary>
        /// 外部子单编号
        /// </summary>
        public String outSubOrderNo { get; set; }

        /// <summary>
        /// 原消费分账时的分账子订单编号，退款分账时必填
        /// </summary>
        public String origOutSubOrderNo { get; set; }

        /// <summary>
        /// 分账金额
        /// </summary>
        [JsonConverter(typeof(AmountToCentJsonConverter))]
        public Decimal amount { get; set; }

        /// <summary>
        /// 结算周期
        /// </summary>
        public String settlePeriod { get; set; }
    }
}
