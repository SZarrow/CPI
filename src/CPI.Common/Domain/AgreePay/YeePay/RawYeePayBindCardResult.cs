using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Domain.AgreePay.YeePay
{
    public class RawYeePayBindCardResult : RawYeePayCommonResult
    {
        public String requestno { get; set; }
        public String status { get; set; }
    }
}
