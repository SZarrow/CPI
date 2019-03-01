using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Domain.FundOut.YeePay
{
    public class YeePaySinglePayResponse : CommonResponse
    {
        public String OutTradeNo { get; set; }
        public String BatchNo { get; set; }
    }
}
