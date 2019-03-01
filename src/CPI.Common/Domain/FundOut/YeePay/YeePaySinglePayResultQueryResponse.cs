using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Domain.FundOut.YeePay
{
    public class YeePaySinglePayResultQueryResponse
    {
        public Int32 PageIndex { get; set; }
        public Int32 PageSize { get; set; }
        public Int32 TotalCount { get; set; }
        public IEnumerable<YeePaySinglePayStatusResult> Orders { get; set; }
    }
}
