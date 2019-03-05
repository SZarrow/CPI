using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Domain.AgreePay.YeePay
{
    public class RawYeePayAgreePayPaymentResult : RawYeePayCommonResult
    {
        public String requestno { get; set; }
        public String amount { get; set; }
        public String status { get; set; }
    }
}
