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
        /// 外部跟踪编号
        /// </summary>
        [Column("out_trade_no")]
        public String OutTradeNo { get; set; }
        /// <summary>
        /// 分账总金额
        /// </summary>
        [Column("total_amount")]
        public Decimal TotalAmount { get; set; }
        /// <summary>
        /// 手续费分账方Id
        /// </summary>
        [Column("fee_payer_id")]
        public String FeePayerId { get; set; }
        /// <summary>
        /// 分账信息
        /// </summary>
        [Column("sharing_info")]
        public String SharingInfo { get; set; }
        /// <summary>
        /// 分账类型：消费分账，退货分账
        /// </summary>
        [Column("sharing_type")]
        public String SharingType { get; set; }
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
