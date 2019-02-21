using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Lotus.Validation;

namespace CPI.Common.Domain.SettleDomain.Bill99.v1_0
{
    public class PersonalRegisterInfoQueryRequestV1 : ValidateModel
    {
        [Required(ErrorMessage = "AppId字段必需")]
        public String AppId { get; set; }
        [Required(ErrorMessage = "UserId字段必需")]
        public String UserId { get; set; }
    }
}
