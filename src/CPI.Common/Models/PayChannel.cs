using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace CPI.Common.Models
{
    /// <summary>
    /// 支付通道表
    /// </summary>
    [Table("pay_channel")]
    public class PayChannel
    {
        /// <summary>
        /// 系统主键
        /// </summary>
        [Column("id")]
        public Int32 Id { get; set; }
        /// <summary>
        /// 通道编码
        /// </summary>
        [Column("channel_code")]
        public String ChannelCode { get; set; }
        /// <summary>
        /// 通道名称
        /// </summary>
        [Column("channel_name")]
        public String ChannelName { get; set; }
        /// <summary>
        /// 通道支付费率
        /// </summary>
        [Column("pay_rate")]
        public Decimal PayRate { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        [Column("remark")]
        public String Remark { get; set; }
    }
}
