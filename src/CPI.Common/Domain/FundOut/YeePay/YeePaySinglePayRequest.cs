using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Lotus.Validation;

namespace CPI.Common.Domain.FundOut.YeePay
{
    public class YeePaySinglePayRequest : ValidateModel
    {
        [Required(ErrorMessage = "OrderNo必需")]
        public String OrderNo { get; set; }
        [Required(ErrorMessage = "Amount必需")]
        public Decimal Amount { get; set; }
        public String AccountName { get; set; }
        public String BankCardNo { get; set; }
        public String BankCode { get; set; }
        public String FeeType { get; set; } = "TARGET";
        public String Remark { get; set; }
    }
}
