using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPI.Config;
using ATBase.Security;

namespace CPI.Utils
{
    public static class MerchantUtil
    {
        public static String MD5Sign(Dictionary<String, String> dic)
        {
            var orderedValues = from t0 in dic
                                where String.Compare(t0.Key, "sign", true) != 0
                                orderby t0.Key
                                select $"{t0.Key}={t0.Value}";

            String signContent = String.Join("&", orderedValues) + $"&{KeyConfig.HehuaMerchantSignKey}";
            return CryptoHelper.GetMD5(signContent).Value;
        }
    }
}
