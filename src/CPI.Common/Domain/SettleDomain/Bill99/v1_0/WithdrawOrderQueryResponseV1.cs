using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Domain.SettleDomain.Bill99.v1_0
{
    public class WithdrawOrderQueryResponseV1 : CommonResponse
    {
        public String PayeeId { get; set; }
        public String IsPlatformPayee { get; set; }
        public String OutTradeNo { get; set; }
        public Decimal OrderAmount { get; set; }
        public String PayMode { get; set; }
        public DateTime TradeBeginTime { get; set; }
        public DateTime? TradeEndTime { get; set; }
        public String Remark { get; set; }
        public String OrderType { get; set; }
    }
}
