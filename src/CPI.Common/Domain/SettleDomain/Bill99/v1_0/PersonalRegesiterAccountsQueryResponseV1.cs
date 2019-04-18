using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Domain.SettleDomain.Bill99.v1_0
{
    public class PersonalRegesiterAccountsQueryResponseV1 : CommonResponse
    {
        public IEnumerable<PersonalRegesiterAccountInfo> PersonalRegesiterAccounts { get; set; }
    }

    public class PersonalRegesiterAccountInfo
    {
        public String AppId { get; set; }
        public String UserId { get; set; }
        public String IDCardNo { get; set; }
        public String RealName { get; set; }
        public String Mobile { get; set; }
    }
}
