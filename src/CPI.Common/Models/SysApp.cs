using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace CPI.Common.Models
{
    /// <summary>
    /// 系统应用表
    /// </summary>
    [Table("sys_app")]
    public class SysApp
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
        /// 应用的名称
        /// </summary>
        [Column("app_name")]
        public String AppName { get; set; }
        /// <summary>
        /// 应用系统的RSA公钥
        /// </summary>
        [Column("app_rsa_public_key")]
        public String AppRSAPublicKey { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        [Column("remark")]
        public String Remark { get; set; }
    }
}
