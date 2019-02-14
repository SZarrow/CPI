using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Lotus.Core;
using Lotus.Validation;
using Newtonsoft.Json;

namespace CPI.Common.Domain.SettleDomain.Bill99.v1_0
{
    public class SharingInfo : ValidateModel
    {
        [Required]
        [RegularExpression("^0|1$", ErrorMessage = "SharingType字段取值只能为0或1")]
        public String SharingType { get; set; }
        [Required]
        [RegularExpression("^0|1$", ErrorMessage = "FeeMode字段取值只能为0或1")]
        public String FeeMode { get; set; }
        [Required]
        public String FeePayerId { get; set; }
        [Required]
        public String SharingData { get; set; }
    }
}
