using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace CPI.Utils
{
    public static class WebUtil
    {
        public static String UrlEncode(String value)
        {
            return HttpUtility.UrlEncode(value).Replace("+", "%20");
        }
    }
}
