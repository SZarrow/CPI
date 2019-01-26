using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Newtonsoft.Json;

namespace CPI.Common.Models
{
    /// <summary>
    /// 分账个人账户实体类
    /// </summary>
    [Table("personal_subaccount")]
    public class PersonalSubAccount
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
        /// 平台用户ID
        /// </summary>
        [Column("uid")]
        public String UID { get; set; }
        /// <summary>
        /// 身份证号
        /// </summary>
        [Column("idcard_no")]
        public String IDCardNo { get; set; }
        /// <summary>
        /// 银行卡类型
        /// </summary>
        [Column("idcard_type")]
        public String IDCardType { get; set; }
        /// <summary>
        /// 用户姓名
        /// </summary>
        [Column("realname")]
        public String RealName { get; set; }
        /// <summary>
        /// 银行预留手机号
        /// </summary>
        [Column("mobile")]
        public String Mobile { get; set; }
        /// <summary>
        /// 邮箱
        /// </summary>
        [Column("email")]
        public String Email { get; set; }
        /// <summary>
        /// 开户状态
        /// </summary>
        [Column("status")]
        public String Status { get; set; }
        /// <summary>
        /// 快钱返回的OpenId
        /// </summary>
        [Column("open_id")]
        public String OpenId { get; set; }
        /// <summary>
        /// 更新时间
        /// </summary>
        [Column("update_time")]
        public DateTime UpdateTime { get; set; }
    }
}
