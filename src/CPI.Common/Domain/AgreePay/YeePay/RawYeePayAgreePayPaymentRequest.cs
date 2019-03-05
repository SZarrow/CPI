using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Domain.AgreePay.YeePay
{
    public class RawYeePayAgreePayPaymentRequest
    {
        public String merchantno { get; set; }
        public String requestno { get; set; }
        public String issms { get; set; } = "false";
        public String identityid { get; set; }
        public String identitytype { get; set; }
        public String cardtop { get; set; }
        public String cardlast { get; set; }
        public String amount { get; set; }
        public String productname { get; set; }
        public String requesttime { get; set; }
        public String terminalno { get; set; }
    }
}
