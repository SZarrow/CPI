using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Lotus.Validation;

namespace CPI.Common.Domain.AgreePay.YeePay
{
    public class YeePayAgreePayPaymentRequest : ValidateModel, IPaymentRequest
    {
        /// <summary>
        /// 分配给接入平台的Id
        /// </summary>
        [Required(ErrorMessage = "AppId字段必需")]
        public String AppId { get; set; }

        /// <summary>
        /// 付款人Id
        /// </summary>
        [Required(ErrorMessage = "PayerId字段必需")]
        public String PayerId { get; set; }

        /// <summary>
        /// 终端识别号
        /// </summary>
        [Required(ErrorMessage = "TerminalNo字段必需")]
        public String TerminalNo { get; set; }

        /// <summary>
        /// 交易金额
        /// </summary>
        [Required(ErrorMessage = "Amount字段必需")]
        [RegularExpression(Resources.AmountRegexExpression, ErrorMessage = "支付金额格式错误")]
        public Decimal Amount { get; set; }

        /// <summary>
        /// 支付外部交易号
        /// </summary>
        [Required(ErrorMessage = "OutTradeNo字段必需")]
        public String OutTradeNo { get; set; }

        /// <summary>
        /// 银行卡号
        /// </summary>
        [Required(ErrorMessage = "BankCardNo字段必需")]
        [RegularExpression(@"^\d{10,32}$", ErrorMessage = "银行卡号格式错误")]
        public String BankCardNo { get; set; }

        /// <summary>
        /// 通知地址
        /// </summary>
        public String NotifyUrl { get; set; }

        /// <summary>
        /// 获取支付金额
        /// </summary>
        public Decimal GetPayAmount()
        {
            return this.Amount;
        }
    }
}
