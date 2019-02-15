using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace CPI.Common.Domain.SettleDomain.Bill99.v1_0
{
    public class RawPersonalAccountBalanceQueryResponseV1 : YZTCommonResponse
    {
        public IEnumerable<RawPersonalAccountBalanceData> accountBalanceList { get; set; }
    }
}
