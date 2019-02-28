using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Domain.FundOut.YeePay
{
    public class RawYeePaySinglePayResult : RawYeePayCommonResult
    {
        public String orderId { get; set; }
        public String batchNo { get; set; }
        public String transferStatusCode { get; set; }
    }
}
