using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace CPI.Common.Models
{
    /// <summary>
    /// 银行基本信息表
    /// </summary>
    [Table("bank_baseinfo")]
    public class BankBaseInfo
    {
        /// <summary>
        /// 系统主键
        /// </summary>
        [Column("id")]
        public Int32 Id { get; set; }
        /// <summary>
        /// 银行编码
        /// </summary>
        [Column("bank_code")]
        public String BankCode { get; set; }
        /// <summary>
        /// 银行名称
        /// </summary>
        [Column("bank_name")]
        public String BankName { get; set; }
        /// <summary>
        /// 银行缩写
        /// </summary>
        [Column("bank_shortname")]
        public String BankShortName { get; set; }
    }
}
