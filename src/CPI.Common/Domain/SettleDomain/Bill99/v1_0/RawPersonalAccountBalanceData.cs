using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace CPI.Common.Domain.SettleDomain.Bill99.v1_0
{
    public class RawPersonalAccountBalanceData
    {
        public String accountType { get; set; }
        public String accountName { get; set; }
        [JsonConverter(typeof(AmountFromCentJsonConverter))]
        public Decimal balance { get; set; }
    }
}
