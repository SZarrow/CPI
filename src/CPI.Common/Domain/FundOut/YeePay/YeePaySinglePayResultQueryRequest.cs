using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using ATBase.Validation;

namespace CPI.Common.Domain.FundOut.YeePay
{
    public class YeePaySinglePayResultQueryRequest : ValidateModel
    {
        [Required(ErrorMessage = "AppId字段必需")]
        public String AppId { get; set; }

        [Required(ErrorMessage = "PageIndex字段必需")]
        [RegularExpression(Resources.PageNumberRegexExpression, ErrorMessage = "PageIndex格式错误")]
        public String PageIndex { get; set; }

        [Required(ErrorMessage = "PageSize字段必需")]
        [RegularExpression(Resources.PageNumberRegexExpression, ErrorMessage = "PageSize格式错误")]
        public String PageSize { get; set; }

        public String QueryMode { get; set; } = "QUERY";

        public String OutTradeNo { get; set; }

        public String From { get; set; }
        public String To { get; set; }
    }
}
