using System;
using System.Collections.Generic;
using System.Text;
using CPI.Common;

namespace CPI.Utils
{
    public static class Bill99Util
    {
        public static PayStatus GetAgreepayPayStatus(String responseCode)
        {
            switch (responseCode)
            {
                case "00":
                    return PayStatus.SUCCESS;
                case "C0":
                case "68":
                case "96":
                    return PayStatus.PROCESSING;
            }

            return PayStatus.FAILURE;
        }
    }
}
