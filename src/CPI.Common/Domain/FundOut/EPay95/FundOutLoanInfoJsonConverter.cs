using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Newtonsoft.Json;

namespace CPI.Common.Domain.FundOut.EPay95
{
    /// <summary>
    /// 
    /// </summary>
    public class FundOutLoanInfoJsonConverter : JsonConverter<FundOutLoanInfo>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="objectType"></param>
        /// <param name="existingValue"></param>
        /// <param name="hasExistingValue"></param>
        /// <param name="serializer"></param>
        public override FundOutLoanInfo ReadJson(JsonReader reader, Type objectType, FundOutLoanInfo existingValue, Boolean hasExistingValue, JsonSerializer serializer)
        {
            if (reader.Value is String)
            {
                try
                {
                    var fundOutLoanInfos = JsonConvert.DeserializeObject<FundOutLoanInfo[]>(HttpUtility.UrlDecode(reader.Value.ToString()));
                    if (fundOutLoanInfos != null && fundOutLoanInfos.Length > 0)
                    {
                        return fundOutLoanInfos[0];
                    }
                }
                catch { }
            }

            return existingValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="serializer"></param>
        public override void WriteJson(JsonWriter writer, FundOutLoanInfo value, JsonSerializer serializer)
        {
            writer.WriteValue(JsonConvert.SerializeObject(value));
        }
    }
}
