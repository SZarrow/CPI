using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common
{
    /// <summary>
    /// 
    /// </summary>
    public class StatusToStringJsonConverter : JsonConverter<Int32>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="objectType"></param>
        /// <param name="existingValue"></param>
        /// <param name="hasExistingValue"></param>
        /// <param name="serializer"></param>
        public override Int32 ReadJson(JsonReader reader, Type objectType, Int32 existingValue, Boolean hasExistingValue, JsonSerializer serializer)
        {
            if (Int32.TryParse((reader.Value ?? String.Empty).ToString(), out Int32 result))
            {
                return result;
            }

            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="serializer"></param>
        public override void WriteJson(JsonWriter writer, Int32 value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString().PadLeft(4, '0'));
        }
    }
}
