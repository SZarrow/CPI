using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace CPI.Common.Domain.SettleDomain.Bill99.v1_0
{
    public class RawPersonalAccountBalanceQueryResponseV1 : YZTCommonResponse
    {
        public IEnumerable<PersonalAccountBalanceData> accountBalanceList { get; set; }
    }

    public class PersonalAccountBalanceData
    {
        [JsonProperty("accountType")]
        public String AccountType { get; set; }
        [JsonProperty("accountName")]
        public String AccountName { get; set; }
        [JsonProperty("balance")]
        [JsonConverter(typeof(AmountFromCentJsonConverter))]
        public Decimal Balance { get; set; }
    }
}
