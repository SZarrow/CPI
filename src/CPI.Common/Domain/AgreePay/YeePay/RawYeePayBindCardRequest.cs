using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Domain.AgreePay.YeePay
{
    public class RawYeePayBindCardRequest
    {
        public String merchantno { get; set; }
        public String requestno { get; set; }
        public String validatecode { get; set; }
    }
}
