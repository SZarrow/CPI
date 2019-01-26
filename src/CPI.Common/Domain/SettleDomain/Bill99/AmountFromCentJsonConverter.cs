using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace CPI.Common.Domain.SettleDomain.Bill99
{
    /// <summary>
    /// 
    /// </summary>
    public class AmountFromCentJsonConverter : JsonConverter<Decimal>
    {
        /// <summary>
        /// 将快钱返回的分转成元
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="objectType"></param>
        /// <param name="existingValue"></param>
        /// <param name="hasExistingValue"></param>
        /// <param name="serializer"></param>
        public override Decimal ReadJson(JsonReader reader, Type objectType, Decimal existingValue, Boolean hasExistingValue, JsonSerializer serializer)
        {
            var value = reader.Value;
            if (value is Decimal)
            {
                return ((Decimal)value) * 0.01m;
            }

            if (Decimal.TryParse(value.ToString(), out Decimal result))
            {
                return result * 0.01m;
            }

            return 0m;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="serializer"></param>
        public override void WriteJson(JsonWriter writer, Decimal value, JsonSerializer serializer)
        {
            writer.WriteValue(value);
        }
    }
}
