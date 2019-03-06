using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Domain.AgreePay.YeePay
{
    public class RawYeePayBindCardResponse : RawYeePayCommonResponse
    {
        public String requestno { get; set; }
        public String status { get; set; }
    }
}
