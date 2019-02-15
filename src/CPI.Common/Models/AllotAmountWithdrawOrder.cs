using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Newtonsoft.Json;

namespace CPI.Common.Models
{
    /// <summary>
    /// 分账提现订单
    /// </summary>
    [Table("allot_amount_withdraw_order")]
    public class AllotAmountWithdrawOrder
    {
        /// <summary>
        /// 主键
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
        /// 平台用户ID
        /// </summary>
        [Column("payee_id")]
        public String PayeeId { get; set; }
        /// <summary>
        /// 内部交易编号
        /// </summary>
        [Column("trade_no")]
        public String TradeNo { get; set; }
        /// <summary>
        /// 商户是否是平台，0是，1否
        /// </summary>
        [Column("is_platform_merchant")]
        public String IsPlatformMerchant { get; set; }
        /// <summary>
        /// 主收款方是否是平台，0是，1否
        /// </summary>
        [Column("is_platform_payee")]
        public String IsPlatformPayee { get; set; }
        /// <summary>
        /// 外部订单编号
        /// </summary>
        [Column("out_trade_no")]
        public String OutTradeNo { get; set; }
        /// <summary>
        /// 提现金额
        /// </summary>
        [Column("amount")]
        public Decimal Amount { get; set; }
        /// <summary>
        /// 支付方式
        /// </summary>
        [Column("pay_mode")]
        public String PayMode { get; set; }
        /// <summary>
        /// 结算周期
        /// </summary>
        [Column("settle_period")]
        public String SettlePeriod { get; set; }
        /// <summary>
        /// 会员自付手续费
        /// </summary>
        [Column("customer_fee")]
        public Decimal CustomerFee { get; set; }
        /// <summary>
        /// 商户代付手续费
        /// </summary>
        [Column("merchant_fee")]
        public Decimal MerchantFee { get; set; }
        /// <summary>
        /// 提现状态
        /// </summary>
        [Column("status")]
        public String Status { get; set; }
        /// <summary>
        /// 订单类型
        /// </summary>
        [Column("order_type")]
        public String OrderType { get; set; }
        /// <summary>
        /// 提现申请时间
        /// </summary>
        [Column("apply_time")]
        public DateTime ApplyTime { get; set; }
        /// <summary>
        /// 提现完成时间
        /// </summary>
        [Column("complete_time")]
        public DateTime? CompleteTime { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        [Column("remark")]
        public String Remark { get; set; }
    }
}
