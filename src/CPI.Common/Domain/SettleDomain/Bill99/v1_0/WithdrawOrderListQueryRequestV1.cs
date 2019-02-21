using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Lotus.Validation;

namespace CPI.Common.Domain.SettleDomain.Bill99.v1_0
{
    public class WithdrawOrderListQueryRequestV1 : ValidateModel
    {
        [Required(ErrorMessage = "AppId字段必需")]
        public String AppId { get; set; }

        [Required(ErrorMessage = "PayeeId字段必需")]
        public String PayeeId { get; set; }

        public String Status { get; set; }
        public String Keyword { get; set; }

        [Required(ErrorMessage = "PageIndex字段必需")]
        [Range(1, Int32.MaxValue, ErrorMessage = "PageIndex范围[1-Int32.MaxValue]")]
        public Int32 PageIndex { get; set; }

        [Required(ErrorMessage = "PageSize字段必需")]
        [Range(1, 50, ErrorMessage = "PageSize范围[1-50]")]
        public Int32 PageSize { get; set; }

        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }
}
