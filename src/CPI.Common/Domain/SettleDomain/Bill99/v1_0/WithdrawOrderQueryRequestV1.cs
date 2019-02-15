using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Lotus.Validation;

namespace CPI.Common.Domain.SettleDomain.Bill99.v1_0
{
    public class WithdrawOrderQueryRequestV1 : ValidateModel
    {
        [Required(ErrorMessage = "AppId字段必需")]
        public String AppId { get; set; }

        [Required(ErrorMessage = "OutTradeNo字段必需")]
        public String OutTradeNo { get; set; }

        [Required(ErrorMessage = "QueryMode字段必需")]
        public String QueryMode { get; set; } = "QUERY";
    }
}
