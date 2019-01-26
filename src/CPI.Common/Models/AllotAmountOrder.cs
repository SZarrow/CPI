using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Newtonsoft.Json;

namespace CPI.Common.Models
{
    /// <summary>
    /// 分账订单
    /// </summary>
    [Table("allot_amount_order")]
    public class AllotAmountOrder
    {
        /// <summary>
        /// 主键
        /// </summary>
        [Column("id")]
        [JsonConverter(typeof(Int64ToStringJsonConverter))]
        public Int64 Id { get; set; }
        /// <summary>
        /// 内部交易编号
        /// </summary>
        [Column("trade_no")]
        public String TradeNo { get; set; }
        /// <summary>
        /// 分配给接入平台的Id
        /// </summary>
        [Column("app_id")]
        public String AppId { get; set; }
        /// <summary>
        /// 分账收款方ID
        /// </summary>
        [Column("payee_id")]
        public String PayeeId { get; set; }
        /// <summary>
        /// 提现单外部交易编号
        /// </summary>
        [Column("withdraw_out_trade_no")]
        public String WithdrawOutTradeNo { get; set; }
        /// <summary>
        /// 外部跟踪编号
        /// </summary>
        [Column("out_trade_no")]
        public String OutTradeNo { get; set; }
        /// <summary>
        /// 原消费分账的外部交易编号，当分账类型为退款分账时此字段必填
        /// </summary>
        [Column("original_out_trade_no")]
        public String OriginalOutTradeNo { get; set; }
        /// <summary>
        /// 分账总金额
        /// </summary>
        [Column("total_amount")]
        public Decimal TotalAmount { get; set; }
        /// <summary>
        /// 结算周期
        /// </summary>
        [Column("settle_period")]
        public String SettlePeriod { get; set; }
        /// <summary>
        /// 分账类型：消费分账，退货分账
        /// </summary>
        [Column("allot_type")]
        public String AllotType { get; set; }
        /// <summary>
        /// 分账状态
        /// </summary>
        [Column("status")]
        public String Status { get; set; }
        /// <summary>
        /// 分账申请时间
        /// </summary>
        [Column("apply_time")]
        public DateTime ApplyTime { get; set; }
        /// <summary>
        /// 分账完成时间
        /// </summary>
        [Column("complete_time")]
        public DateTime? CompleteTime { get; set; }
    }
}
