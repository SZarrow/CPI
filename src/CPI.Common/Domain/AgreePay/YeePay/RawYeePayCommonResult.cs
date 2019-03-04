using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Domain.AgreePay.YeePay
{
    public abstract class RawYeePayCommonResult
    {
        public String errorcode { get; set; }
        public String errormsg { get; set; }
    }
}
