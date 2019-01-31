using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Domain.SettleDomain.Bill99.v1_0
{
    /// <summary>
    /// 
    /// </summary>
    public class PersonalBindCardSendSmsValidCodeResponseV1 : CommonResponse
    {
        /// <summary>
        /// 
        /// </summary>
        public String UserId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public String OutTradeNo { get; set; }
    }
}
