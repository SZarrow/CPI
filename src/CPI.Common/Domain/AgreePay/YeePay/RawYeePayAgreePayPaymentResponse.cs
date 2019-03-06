using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Domain.AgreePay.YeePay
{
    public class RawYeePayAgreePayPaymentResponse : RawYeePayCommonResponse
    {
        public String requestno { get; set; }
        public String yborderid { get; set; }
        public String amount { get; set; }
        public String status { get; set; }
    }
}
