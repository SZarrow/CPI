using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Domain.FundOut.YeePay
{
    public class RawYeePaySinglePayResultQueryStatusResult : RawYeePayCommonResult
    {
        public String totalCount { get; set; }
        public String pageNo { get; set; }
        public String pageSize { get; set; }
        public String totalPageSize { get; set; }
        public String list { get; set; }
    }
}
