using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Domain.SettleDomain.Bill99.v1_0
{
    public class RawPersonalAccountBalanceQueryRequestV1
    {
        public String uId { get; set; }
        public String isPlatform { get; set; }
        public String[] accountType { get; set; }
    }
}
