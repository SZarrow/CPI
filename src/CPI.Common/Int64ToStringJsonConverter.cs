using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace CPI.Common
{
    /// <summary>
    /// 
    /// </summary>
    public class Int64ToStringJsonConverter : JsonConverter<Int64>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="objectType"></param>
        /// <param name="existingValue"></param>
        /// <param name="hasExistingValue"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public override Int64 ReadJson(JsonReader reader, Type objectType, Int64 existingValue, Boolean hasExistingValue, JsonSerializer serializer)
        {
            var v = (reader.Value ?? String.Empty).ToString();

            if (Int64.TryParse(v, out Int64 n))
            {
                return n;
            }

            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="serializer"></param>
        public override void WriteJson(JsonWriter writer, Int64 value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }
    }
}
