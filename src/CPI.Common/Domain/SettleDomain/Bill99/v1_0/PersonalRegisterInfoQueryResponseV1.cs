using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Domain.SettleDomain.Bill99.v1_0
{
    public class PersonalRegisterInfoQueryResponseV1
    {
        public String UserId { get; set; }
        public String IDCardNo { get; set; }
        public String IDCardType { get; set; }
        public String RealName { get; set; }
        public String Mobile { get; set; }
        public String Email { get; set; }
        public String Status { get; set; }
    }
}
