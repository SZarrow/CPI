using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CPI.Common.Domain.SettleDomain.Bill99.v1_0
{
    public class AgreepayPayResultPullRequestV1
    {
        [Required(ErrorMessage = "AppId字段必需")]
        public String AppId { get; set; }

        [Required(ErrorMessage = "Count字段必需")]
        public Int32 Count { get; set; }
    }
}
