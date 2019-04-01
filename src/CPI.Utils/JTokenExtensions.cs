using System;
using System.Collections.Generic;
using System.Text;
using ATBase.Core;
using Newtonsoft.Json.Linq;

namespace CPI.Utils
{
    public static class JTokenExtensions
    {
        public static T GetValue<T>(this JToken token, String name)
        {
            if (name.HasValue())
            {
                try
                {
                    return token.Value<T>(name);
                }
                catch { }
            }

            return default(T);
        }
    }
}
