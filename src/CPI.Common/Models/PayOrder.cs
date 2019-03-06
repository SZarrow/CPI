using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Newtonsoft.Json;

namespace CPI.Common.Models
{
    /// <summary>
    /// 支付订单实体类
    /// </summary>
    [Table("pay_order")]
    public class PayOrder
    {
        /// <summary>
        /// 系统主键
        /// </summary>
        [Column("id")]
        [JsonConverter(typeof(Int64ToStringJsonConverter))]
        public Int64 Id { get; set; }
        /// <summary>
        /// 内部交易编号，CPI系统内部全局唯一
        /// </summary>
        [Column("trade_no")]
        public String TradeNo { get; set; }
        /// <summary>
        /// 分配给接入平台的Id
        /// </summary>
        [Column("app_id")]
        public String AppId { get; set; }
        /// <summary>
        /// 付款人Id
        /// </summary>
        [Column("payer_id")]
        public String PayerId { get; set; }
        /// <summary>
        /// 外部交易编号（订单编号）
        /// </summary>
        [Column("out_trade_no")]
        public String OutTradeNo { get; set; }
        /// <summary>
        /// 支付金额
        /// </summary>
        [Column("pay_amount")]
        public Decimal PayAmount { get; set; }
        /// <summary>
        /// 银行卡号
        /// </summary>
        [Column("bankcard_no")]
        public String BankCardNo { get; set; }
        /// <summary>
        /// 手续费
        /// </summary>
        [Column("fee")]
        public Decimal Fee { get; set; }
        /// <summary>
        /// 支付状态
        /// </summary>
        [Column("pay_status")]
        public String PayStatus { get; set; }
        /// <summary>
        /// 支付类型
        /// </summary>
        [Column("pay_type")]
        public String PayType { get; set; }
        /// <summary>
        /// 支付通道编码
        /// </summary>
        [Column("pay_channel_code")]
        public String PayChannelCode { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        [Column("create_time")]
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 最近一次更新时间
        /// </summary>
        [Column("update_time")]
        public DateTime? UpdateTime { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        [Column("remark")]
        public String Remark { get; set; }
    }
}
