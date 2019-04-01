using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using ATBase.Validation;

namespace CPI.Common.Domain.AgreePay.YeePay
{
    public class YeePayAgreePayRefundRequest : ValidateModel
    {
        [Required(ErrorMessage = "AppId字段必需")]
        public String AppId { get; set; }

        [Required(ErrorMessage = "OutTradeNo字段必需")]
        public String OutTradeNo { get; set; }

        [Required(ErrorMessage = "OriginalOutTradeNo字段必需")]
        public String OriginalOutTradeNo { get; set; }

        [Required(ErrorMessage = "Amount字段必需")]
        [RegularExpression(Resources.AmountRegexExpression, ErrorMessage = "Amount字段格式错误")]
        public Decimal Amount { get; set; }

        public String Remark { get; set; }
    }
}
