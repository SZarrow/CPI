using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Lotus.Validation;

namespace CPI.Common.Domain.Common
{
    public class CommonPullRequest : ValidateModel
    {
        [Required(ErrorMessage = "AppId字段必需")]
        public String AppId { get; set; }

        [Required(ErrorMessage = "Count字段必需")]
        public Int32 Count { get; set; }
    }
}
