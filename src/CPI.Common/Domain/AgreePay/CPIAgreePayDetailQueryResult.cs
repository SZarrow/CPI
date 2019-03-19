using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Domain.AgreePay
{
    public class CPIAgreePayDetailQueryResult : CommonResponse
    {
        public String PayerId { get; set; }
        public String IDCardNo { get; set; }
        public String RealName { get; set; }
        public String Mobile { get; set; }
        public String OutTradeNo { get; set; }
        public String BankCardNo { get; set; }
        public Decimal Amount { get; set; }
        public Decimal Fee { get; set; }
        public String PayType { get; set; }
        public String PayChannelCode { get; set; }
        public DateTime CreateTime { get; set; }
        public String Remark { get; set; }
    }
}
