using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Newtonsoft.Json;

namespace CPI.Common.Models
{
    /// <summary>
    /// 提现绑卡信息
    /// </summary>
    [Table("withdraw_bankcard_bindinfo")]
    public class WithdrawBankCardBindInfo
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
        /// 收款人Id
        /// </summary>
        [Column("payee_id")]
        public String PayeeId { get; set; }
        /// <summary>
        /// 银行卡号
        /// </summary>
        [Column("bankcard_no")]
        public String BankCardNo { get; set; }
        /// <summary>
        /// 手机号
        /// </summary>
        [Column("mobile")]
        public String Mobile { get; set; }
        /// <summary>
        /// 银行卡标记，0表示一类卡，1表示二类卡
        /// </summary>
        [Column("bankcard_flag")]
        public Int32 BankCardFlag { get; set; }
        /// <summary>
        /// 绑定状态
        /// </summary>
        [Column("bind_status")]
        public String BindStatus { get; set; }
        /// <summary>
        /// 申请绑卡时间
        /// </summary>
        [Column("apply_time")]
        public DateTime ApplyTime { get; set; }
        /// <summary>
        /// 快钱侧的银行卡主键Id，绑卡成功时返回
        /// </summary>
        [Column("member_bankaccount_id")]
        public String MemberBankAccountId { get; set; }
    }
}
