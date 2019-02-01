using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace CPI.WebAPI
{
    public static class IPAddressExtensions
    {
        public static String ToIPString(this IPAddress ipAddr)
        {
            return ipAddr.ToString() == "::1" ? "127.0.0.1" : ipAddr.ToString();
        }
    }
}
