using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace CPI.Common.Domain.SettleDomain.Bill99
{
    /// <summary>
    /// 分账结果
    /// </summary>
    public class SettleResult
    {
        /// <summary>
        /// 外部子订单号
        /// </summary>
        [JsonProperty("outSubOrderNo")]
        public String OutSubOrderNo { get; set; }

        /// <summary>
        /// 交易类型
        /// </summary>
        [JsonProperty("txnType")]
        public Int32 TxnType { get; set; }

        /// <summary>
        /// 分账金额
        /// </summary>
        [JsonProperty("amount")]
        [JsonConverter(typeof(AmountFromCentJsonConverter))]
        public Decimal Amount { get; set; }

        /// <summary>
        /// 结算周期
        /// </summary>
        [JsonProperty("settlePeriod")]
        public String SettlePeriod { get; set; }

        /// <summary>
        /// 结算状态，0：初始化, 8：结算失败, 9：结算成功
        /// </summary>
        [JsonProperty("settleStatus")]
        public String SettleStatus { get; set; }
    }
}
