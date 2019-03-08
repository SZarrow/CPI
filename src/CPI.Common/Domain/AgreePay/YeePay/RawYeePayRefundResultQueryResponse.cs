using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Domain.AgreePay.YeePay
{
    public class RawYeePayRefundResultQueryResponse : RawYeePayCommonResponse
    {
        public String requestno { get; set; }
        public String yborderid { get; set; }
        public String status { get; set; }
        public Decimal amount { get; set; }
        public String cardtop { get; set; }
        public String cardlast { get; set; }
        public String bankcode { get; set; }
    }
}
