using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using ATBase.Core;

namespace CPI.Utils
{
    public static class WebUtil
    {
        public static String UrlEncode(String value)
        {
            if (value.HasValue())
            {
                return HttpUtility.UrlEncode(value).Replace("+", "%20");
            }

            return value;
        }
    }
}
