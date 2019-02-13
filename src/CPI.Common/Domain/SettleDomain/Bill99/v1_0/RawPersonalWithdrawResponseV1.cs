using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Domain.SettleDomain.Bill99.v1_0
{
    public class RawPersonalWithdrawResponseV1 : YZTCommonResponse
    {
        public String outTradeNo { get; set; }
        public String status { get; set; }
    }
}
