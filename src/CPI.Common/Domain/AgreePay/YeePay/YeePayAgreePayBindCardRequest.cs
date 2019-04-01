using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using ATBase.Validation;

namespace CPI.Common.Domain.AgreePay.YeePay
{
    public class YeePayAgreePayBindCardRequest : ValidateModel
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
        /// 外部交易号（商户订单号）
        /// </summary>
        [Required(ErrorMessage = "OutTradeNo字段必需")]
        public String OutTradeNo { get; set; }

        /// <summary>
        /// 绑卡银行卡号
        /// </summary>
        [Required(ErrorMessage = "BankCardNo字段必需")]
        public String BankCardNo { get; set; }

        /// <summary>
        /// 付款人银行预留手机号
        /// </summary>
        public String Mobile { get; set; }

        /// <summary>
        /// 短信验证码
        /// </summary>
        [Required(ErrorMessage = "SmsValidCode字段必需")]
        public String SmsValidCode { get; set; }
    }
}
