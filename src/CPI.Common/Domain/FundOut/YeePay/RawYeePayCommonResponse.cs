using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Domain.FundOut.YeePay
{
    public class RawYeePayCommonResponse<TResult>
    {
        public String state { get; set; }
        public TResult result { get; set; }
        public String ts { get; set; }
        public String sign { get; set; }
    }
}
