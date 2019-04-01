using System;
using System.Collections.Generic;
using System.Text;
using ATBase.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CPI.Utils
{
    public static class JsonUtil
    {
        public static XResult<String> SerializeObject(Object value)
        {
            if (value == null)
            {
                return new XResult<String>(null, new ArgumentNullException(nameof(value)));
            }

            try
            {
                return new XResult<String>(JsonConvert.SerializeObject(value));
            }
            catch (Exception ex)
            {
                return new XResult<String>(null, ex);
            }
        }

        public static XResult<T> DeserializeObject<T>(String value)
        {
            if (value.IsNullOrWhiteSpace())
            {
                return new XResult<T>(default(T), new ArgumentNullException(nameof(value)));
            }

            try
            {
                return new XResult<T>(JsonConvert.DeserializeObject<T>(value));
            }
            catch (Exception ex)
            {
                return new XResult<T>(default(T), ex);
            }
        }

        private static readonly JToken DefaultJToken = JToken.Parse("{}");

        public static JToken GetJToken(String jsonString)
        {
            if (jsonString.IsNullOrWhiteSpace())
            {
                return DefaultJToken;
            }

            try
            {
                return JToken.Parse(jsonString, new JsonLoadSettings()
                {
                    LineInfoHandling = LineInfoHandling.Ignore
                });
            }
            catch (Exception)
            {
                return DefaultJToken;
            }
        }
    }
}
