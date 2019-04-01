using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using ATBase.Validation;

namespace CPI.Common.Domain.SettleDomain.Bill99
{
    /// <summary>
    /// 提现手续费查询请求类
    /// </summary>
    public class WithdrawQueryFeeRequest : ValidateModel
    {
        /// <summary>
        /// 分配给接入平台的Id
        /// </summary>
        [Required(ErrorMessage = "AppId字段必需")]
        public String AppId { get; set; }
        /// <summary>
        /// 收款人Id
        /// </summary>
        [Required(ErrorMessage = "PayeeId字段必需")]
        public String PayeeId { get; set; }
        /// <summary>
        /// 提现金额
        /// </summary>
        [Required(ErrorMessage = "Amount字段必需")]
        public Decimal Amount { get; set; }
    }
}
