using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace CPI.Common.Models
{
    /// <summary>
    /// 通道路由表
    /// </summary>
    [Table("app_channel_route")]
    public class AppChannelRoute
    {
        /// <summary>
        /// 系统主键
        /// </summary>
        [Column("id")]
        public Int32 Id { get; set; }
        /// <summary>
        /// 应用的Id
        /// </summary>
        [Column("app_id")]
        public String AppId { get; set; }
        /// <summary>
        /// 支付通道编码
        /// </summary>
        [Column("pay_channel_code")]
        public String PayChannelCode { get; set; }
    }
}
