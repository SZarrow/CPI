using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Domain.AgreePay.YeePay
{
    public class RawYeePayRefundResponse : RawYeePayCommonResponse
    {
        public String merchantno { get; set; }
        public String requestno { get; set; }
        public String yborderid { get; set; }
        public String status { get; set; }
        public String amount { get; set; }
    }
}
