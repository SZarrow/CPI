using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace CPI.Common.Domain.SettleDomain.Bill99
{
    /// <summary>
    /// 金额元转换成分
    /// </summary>
    public class AmountToCentJsonConverter : JsonConverter<Decimal>
    {
        /// <summary>
        /// 读取元
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="objectType"></param>
        /// <param name="existingValue"></param>
        /// <param name="hasExistingValue"></param>
        /// <param name="serializer"></param>
        public override Decimal ReadJson(JsonReader reader, Type objectType, Decimal existingValue, Boolean hasExistingValue, JsonSerializer serializer)
        {
            if (reader.Value != null)
            {
                if (reader.Value is Decimal)
                {
                    return (Decimal)reader.Value;
                }

                if (Decimal.TryParse(reader.Value.ToString(), out Decimal value))
                {
                    return value;
                }
            }

            return 0m;
        }

        /// <summary>
        /// 将元写成分
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="serializer"></param>
        public override void WriteJson(JsonWriter writer, Decimal value, JsonSerializer serializer)
        {
            writer.WriteValue(Convert.ToInt32(value * 100));
        }
    }
}
