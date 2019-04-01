using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using ATBase.Validation;

namespace CPI.Common.Domain.SettleDomain.Bill99.v1_0
{
    public class PersonalWithdrawResultPullRequestV1 : ValidateModel
    {
        [Required(ErrorMessage = "AppId字段")]
        public String AppId { get; set; }

        [Required(ErrorMessage = "Count字段必需")]
        public Int32 Count { get; set; }
    }
}
