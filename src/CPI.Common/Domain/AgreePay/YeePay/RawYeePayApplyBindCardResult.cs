using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Domain.AgreePay.YeePay
{
    public class RawYeePayApplyBindCardResult : RawYeePayCommonResult
    {
        public String merchantno { get; set; }
        public String requestno { get; set; }
        public String yborderid { get; set; }
        public String status { get; set; }
        public String issms { get; set; }
        public String bankcode { get; set; }
        public String smscode { get; set; }
        public String codesender { get; set; }
        public String smstype { get; set; }
        public String advicesmstype { get; set; } = "MESSAGE";
    }
}
