using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace CPI.Common.Domain.SettleDomain.Bill99
{
    /// <summary>
    /// 
    /// </summary>
    public class RawAllotAmountPayRequest
    {
        /// <summary>
        /// 外部订单编号
        /// </summary>
        public String outOrderNo { get; set; }

        /// <summary>
        /// 分账总金额
        /// </summary>
        [JsonConverter(typeof(AmountToCentJsonConverter))]
        public Decimal totalAmount { get; set; }

        /// <summary>
        /// 分账数据
        /// </summary>
        public RawSettleData[] settleData { get; set; }
    }
}
