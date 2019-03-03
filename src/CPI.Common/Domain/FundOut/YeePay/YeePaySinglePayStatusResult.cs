using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Domain.FundOut.YeePay
{
    public class YeePaySinglePayStatusResult : CommonResponse
    {
        public String OutTradeNo { get; set; }
        public String BankCardNo { get; set; }
        public Decimal Amount { get; set; }
        public Decimal Fee { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
