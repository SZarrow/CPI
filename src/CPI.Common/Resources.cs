using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common
{
    public static class Resources
    {
        public const String PageNumberRegexExpression = @"^[1-9]\d*?$";
        public const String AmountRegexExpression = @"^(\d+\.\d{1,2}|\d+?)$";
    }
}
