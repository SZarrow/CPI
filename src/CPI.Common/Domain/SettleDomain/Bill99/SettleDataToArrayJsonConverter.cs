using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace CPI.Common.Domain.SettleDomain.Bill99
{
    /// <summary>
    /// SettleData转成SettleData[]格式的转换器
    /// </summary>
    public class SettleDataToArrayJsonConverter : JsonConverter<SettleData>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="objectType"></param>
        /// <param name="existingValue"></param>
        /// <param name="hasExistingValue"></param>
        /// <param name="serializer"></param>
        public override SettleData ReadJson(JsonReader reader, Type objectType, SettleData existingValue, Boolean hasExistingValue, JsonSerializer serializer)
        {
            var value = reader.Value;
            if (value is SettleData[])
            {
                var datas = value as SettleData[];
                if (datas.Length == 1)
                {
                    return datas[0];
                }
            }

            return value as SettleData;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="serializer"></param>
        public override void WriteJson(JsonWriter writer, SettleData value, JsonSerializer serializer)
        {
            writer.WriteValue(JsonConvert.SerializeObject(new SettleData[] { value }));
        }
    }
}
