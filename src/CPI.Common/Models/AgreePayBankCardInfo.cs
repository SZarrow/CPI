using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Newtonsoft.Json;

namespace CPI.Common.Models
{
    /// <summary>
    /// 银行卡信息表
    /// </summary>
    [Table("agreepay_bankcard_info")]
    public class AgreePayBankCardInfo
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
        /// 付款人真实姓名
        /// </summary>
        [Column("realname")]
        public String RealName { get; set; }
        /// <summary>
        /// 付款人身份证号
        /// </summary>
        [Column("idcard_no")]
        public String IDCardNo { get; set; }
        /// <summary>
        /// 银行卡号
        /// </summary>
        [Column("bankcard_no")]
        public String BankCardNo { get; set; }
        /// <summary>
        /// 银行绑定手机号
        /// </summary>
        [Column("mobile")]
        public String Mobile { get; set; }
        /// <summary>
        /// 银行编码
        /// </summary>
        [Column("bank_code")]
        public String BankCode { get; set; }
        /// <summary>
        /// 更新时间
        /// </summary>
        [Column("update_time")]
        public DateTime UpdateTime { get; set; }
    }
}
