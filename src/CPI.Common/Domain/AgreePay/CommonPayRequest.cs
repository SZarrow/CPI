using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Lotus.Validation;

namespace CPI.Common.Domain.AgreePay
{
    /// <summary>
    /// 协议支付通用请求类
    /// </summary>
    public class CommonPayRequest : ValidateModel, IPaymentRequest
    {
        /// <summary>
        /// 付款人ID
        /// </summary>
        [Required(ErrorMessage = "PayerId字段必需")]
        public String PayerId { get; set; }
        /// <summary>
        /// 付款金额
        /// </summary>
        [Required(ErrorMessage = "Amount字段必需")]
        public Decimal Amount { get; set; }
        /// <summary>
        /// 外部交易编号
        /// </summary>
        [Required(ErrorMessage = "OutTradeNo字段必需")]
        public String OutTradeNo { get; set; }
        /// <summary>
        /// 银行卡号
        /// </summary>
        [Required(ErrorMessage = "BankCardNo")]
        public String BankCardNo { get; set; }

        /// <summary>
        /// 分账类型
        /// </summary>
        [Required(ErrorMessage = "SharingType字段必需")]
        [RegularExpression("^0|1$", ErrorMessage = "SharingType字段格式错误")]
        public String SharingType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Required(ErrorMessage = "FeePayerId字段必需")]
        public String FeePayerId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Required(ErrorMessage = "Fee字段必需")]
        public Decimal Fee { get; set; }

        /// <summary>
        /// 分账周期
        /// </summary>
        [Required(ErrorMessage = "SharingPeriod字段必需")]
        [RegularExpression(@"^T\+\d$", ErrorMessage = "SharingPeriod字段格式错误")]
        public String SharingPeriod { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Decimal GetPayAmount()
        {
            return this.Amount;
        }
    }
}
