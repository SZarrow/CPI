using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Lotus.Validation;

namespace CPI.Common.Domain.SettleDomain.Bill99
{
    /// <summary>
    /// 个人提现绑卡状态查询请求类
    /// </summary>
    public class PersonalBindCardStatusQueryRequest : ValidateModel
    {
        /// <summary>
        /// 收款人Id
        /// </summary>
        [Required(ErrorMessage = "PayeeId字段必需")]
        public String PayeeId { get; set; }
        /// <summary>
        /// 银行卡号
        /// </summary>
        [Required(ErrorMessage = "BankCardNo字段必需")]
        public String BankCardNo { get; set; }
    }
}
