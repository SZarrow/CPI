using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using ATBase.Core;
using ATBase.Validation;
using Newtonsoft.Json;

namespace CPI.Common.Domain.SettleDomain.Bill99.v1_0
{
    public class SharingInfo : ValidateModel
    {
        [Required(ErrorMessage = "SharingType字段必需")]
        [RegularExpression("^0|1$", ErrorMessage = "SharingType字段取值只能为0或1")]
        public String SharingType { get; set; }
        [Required(ErrorMessage = "FeeMode字段必需")]
        [RegularExpression("^0|1$", ErrorMessage = "FeeMode字段取值只能为0或1")]
        public String FeeMode { get; set; }
        [Required(ErrorMessage = "FeePayerId字段必需")]
        public String FeePayerId { get; set; }
        [Required(ErrorMessage = "SharingData字段必需")]
        public String SharingData { get; set; }
    }
}
