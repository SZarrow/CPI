using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace CPI.Common.Domain.SettleDomain.Bill99
{
    /// <summary>
    /// 分账明细查询响应类
    /// </summary>
    public class AllotAmountResultQueryResponse : YZTCommonResponse
    {
        /// <summary>
        /// 外部订单号
        /// </summary>
        [JsonProperty("outOrderNo")]
        public String OutOrderNo { get; set; }

        /// <summary>
        /// 交易类型，1：消费，2：退货
        /// </summary>
        [JsonProperty("txnType")]
        public String TxnType { get; set; }

        /// <summary>
        /// 分账总金额
        /// </summary>
        [JsonProperty("totalAmount")]
        [JsonConverter(typeof(AmountFromCentJsonConverter))]
        public Decimal TotalAmount { get; set; }

        /// <summary>
        /// 分账结果
        /// </summary>
        [JsonProperty("settleResult")]
        public IEnumerable<SettleResult> SettleResults { get; set; }
    }
}
