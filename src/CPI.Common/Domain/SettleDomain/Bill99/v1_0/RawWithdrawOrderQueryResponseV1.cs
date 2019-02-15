using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Domain.SettleDomain.Bill99.v1_0
{
    public class RawWithdrawOrderQueryResponseV1 : YZTCommonResponse
    {
        public String payeeUId { get; set; }
        public String isPlatformPayee { get; set; }
        public String outTradeNo { get; set; }
        public String billOrderNo { get; set; }
        public Decimal orderAmount { get; set; }
        public String payMode { get; set; }
        public DateTime txnBeginTime { get; set; }
        public DateTime txnEndTime { get; set; }
        public String memo { get; set; }
        public String orderStatus { get; set; }
        public String orderType { get; set; }
    }
}
