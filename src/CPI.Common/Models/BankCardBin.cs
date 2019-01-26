using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace CPI.Common.Models
{
    /// <summary>
    /// 银行卡Bin表
    /// </summary>
    [Table("bankcard_bin")]
    public class BankCardBin
    {
        /// <summary>
        /// 系统主键
        /// </summary>
        [Column("id")]
        public Int32 Id { get; set; }
        /// <summary>
        /// 银行卡Bin号
        /// </summary>
        [Column("card_bin")]
        public String CardBin { get; set; }
        /// <summary>
        /// 银行名称
        /// </summary>
        [Column("bank_name")]
        public String BankName { get; set; }
        /// <summary>
        /// 银行编码
        /// </summary>
        [Column("bank_code")]
        public String BankCode { get; set; }
        /// <summary>
        /// 银行卡名称
        /// </summary>
        [Column("card_name")]
        public String CardName { get; set; }
    }
}
