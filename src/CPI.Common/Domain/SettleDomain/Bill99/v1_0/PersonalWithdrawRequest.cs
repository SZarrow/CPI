using System;
using Lotus.Validation;

namespace CPI.Common.Domain.SettleDomain.Bill99.v1_0
{
    public class PersonalWithdrawRequest : ValidateModel
    {
        public String FunctionCode { get; set; }
        public String OutTradeNo { get; set; }
        public String MerchantName { get; set; }
        public String MerchantUId { get; set; }
        public String IsPlatformMerchant { get; set; }
        public String IDCardName { get; set; }
        public Decimal Amount { get; set; }
        public String BankCardNo { get; set; }
        public String BankName { get; set; }
    }
}
