using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using CPI.Common.Properties;
using Lotus.Validation;

namespace CPI.Common.Domain.FundOut.YeePay
{
    public class YeePaySinglePayRequest : ValidateModel
    {
        [Required(ErrorMessage = "AppId字段必需")]
        public String AppId { get; set; }

        [Required(ErrorMessage = "OutTradeNo字段必需")]
        public String OutTradeNo { get; set; }

        [Required(ErrorMessage = "Amount字段必需")]
        [RegularExpression(Resources.AmountRegexExpression, ErrorMessage = "Amount格式错误")]
        public String Amount { get; set; }

        [Required(ErrorMessage = "AccountName字段必需")]
        public String AccountName { get; set; }

        [Required(ErrorMessage = "BankCardNo字段必需")]
        public String BankCardNo { get; set; }

        [Required(ErrorMessage = "BankCode字段必需")]
        public String BankCode { get; set; }

        [Required(ErrorMessage = "FeeType字段必需")]
        public String FeeType { get; set; } = "TARGET";

        public String Remark { get; set; }
    }
}
