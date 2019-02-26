using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Domain.FundOut.YeePay
{
    public class RawYeePaySinglePayRequest
    {
        public String customerNumber { get; set; }
        public String groupNumber { get; set; }
        public String batchNo { get; set; }
        public String orderId { get; set; }
        public String amount { get; set; }
        public String accountName { get; set; }
        public String accountNumber { get; set; }
        public String bankCode { get; set; }
        public String feeType { get; set; }
    }
}
