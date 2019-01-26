using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Newtonsoft.Json;

namespace CPI.Common.Models
{
    /// <summary>
    /// 银行卡绑卡信息表
    /// </summary>
    [Table("agreepay_bankcard_bindinfo")]
    public class AgreePayBankCardBindInfo
    {
        /// <summary>
        /// 系统主键
        /// </summary>
        [Column("id")]
        [JsonConverter(typeof(Int64ToStringJsonConverter))]
        public Int64 Id { get; set; }
        /// <summary>
        /// 分配给接入平台的Id
        /// </summary>
        [Column("app_id")]
        public String AppId { get; set; }
        /// <summary>
        /// 付款人ID
        /// </summary>
        [Column("payer_id")]
        public String PayerId { get; set; }
        /// <summary>
        /// 银行卡Id
        /// </summary>
        [Column("bankcard_id")]
        public Int64 BankCardId { get; set; }
        /// <summary>
        /// 外部交易编号
        /// </summary>
        [Column("out_trade_no")]
        public String OutTradeNo { get; set; }
        /// <summary>
        /// 银行卡号
        /// </summary>
        [Column("bankcard_no")]
        public String BankCardNo { get; set; }
        /// <summary>
        /// 支付通道编码
        /// </summary>
        [Column("pay_channel_code")]
        public String PayChannelCode { get; set; }
        /// <summary>
        /// 支付令牌
        /// </summary>
        [Column("pay_token")]
        public String PayToken { get; set; }
        /// <summary>
        /// 绑定状态
        /// </summary>
        [Column("bind_status")]
        public String BindStatus { get; set; }
        /// <summary>
        /// 申请绑定时间
        /// </summary>
        [Column("apply_time")]
        public DateTime ApplyTime { get; set; }
    }
}
