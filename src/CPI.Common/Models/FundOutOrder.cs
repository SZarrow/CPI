using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Newtonsoft.Json;

namespace CPI.Common.Models
{
    /// <summary>
    /// 代付订单
    /// </summary>
    [Table("fundout_order")]
    public class FundOutOrder
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
        /// 收款人姓名
        /// </summary>
        [Column("realname")]
        public String RealName { get; set; }
        /// <summary>
        /// 收款人手机号
        /// </summary>
        [Column("mobile")]
        public String Mobile { get; set; }
        /// <summary>
        /// 收款金额
        /// </summary>
        [Column("amount")]
        public Decimal Amount { get; set; }
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
        /// 手续费付费方式
        /// </summary>
        [Column("fee_action")]
        public String FeeAction { get; set; }
        /// <summary>
        /// 银行名称
        /// </summary>
        [Column("bank_name")]
        public String BankName { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        [Column("remark")]
        public String Remark { get; set; }
        /// <summary>
        /// 申请时间
        /// </summary>
        [Column("apply_time")]
        public DateTime? ApplyTime { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        [Column("end_time")]
        public DateTime? EndTime { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        [Column("create_time")]
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 更新时间
        /// </summary>
        [Column("update_time")]
        public DateTime? UpdateTime { get; set; }
    }
}
