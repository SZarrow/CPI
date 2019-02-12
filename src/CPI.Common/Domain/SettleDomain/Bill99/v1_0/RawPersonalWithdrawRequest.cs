using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace CPI.Common.Domain.SettleDomain.Bill99.v1_0
{
    public class RawPersonalWithdrawRequest
    {
        public String functionCode { get; set; }
        public String outTradeNo { get; set; }
        public String merchantName { get; set; }
        public String merchantUId { get; set; }
        public String isPlatformMerchant { get; set; }
        public String bankAcctName { get; set; }
        [JsonConverter(typeof(AmountToCentJsonConverter))]
        public Decimal amount { get; set; }
        public String bankAcctId { get; set; }
        public String bankName { get; set; }
    }
}
