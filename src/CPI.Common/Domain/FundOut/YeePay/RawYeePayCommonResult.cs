using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Domain.FundOut.YeePay
{
    public abstract class RawYeePayCommonResult
    {
        public String errorCode { get; set; }
        public String errorMsg { get; set; }
    }
}