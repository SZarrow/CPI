using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Domain.FundOut.YeePay
{
    public class RawYeePaySinglePayResultQueryStatusRequest
    {
        public String customerNumber { get; set; }
        public String batchNo { get; set; }
        public String orderId { get; set; }
        public String product { get; set; } = String.Empty;
        public String pageNo { get; set; }
        public String pageSize { get; set; }
    }
}
