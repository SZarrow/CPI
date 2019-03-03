using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Domain.FundOut.YeePay
{
    public class RawYeePaySinglePayResultDetail
    {
        public String orderId { get; set; }
        public String batchNo { get; set; }
        public String accountName { get; set; }
        public String accountNumber { get; set; }
        public String amount { get; set; }
        public String bankCode { get; set; }
        public String bankName { get; set; }
        public String bankTrxStatusCode { get; set; }
        public String fee { get; set; }
        public String feeType { get; set; }
        public String leaveWord { get; set; }
        public String transferStatusCode { get; set; }
        public String urgency { get; set; }
        public String urgencyType { get; set; }
    }
}
