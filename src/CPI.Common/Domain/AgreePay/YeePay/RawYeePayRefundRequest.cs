using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Domain.AgreePay.YeePay
{
    public class RawYeePayRefundRequest
    {
        public String merchantno { get; set; }
        public String requestno { get; set; }
        public String paymentyborderid { get; set; }
        public String amount { get; set; }
        public String requesttime { get; set; }
        public String remark { get; set; }
    }
}
