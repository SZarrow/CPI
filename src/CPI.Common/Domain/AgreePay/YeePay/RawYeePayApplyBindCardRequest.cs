using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Domain.AgreePay.YeePay
{
    public class RawYeePayApplyBindCardRequest
    {
        public String merchantno { get; set; }
        public String requestno { get; set; }
        public String identityid { get; set; }
        public String identitytype { get; set; }
        public String cardno { get; set; }
        public String idcardno { get; set; }
        public String idcardtype { get; set; }
        public String username { get; set; }
        public String phone { get; set; }
        public Boolean issms { get; set; }
        public String requesttime { get; set; }
        public String authtype { get; set; }
    }
}
