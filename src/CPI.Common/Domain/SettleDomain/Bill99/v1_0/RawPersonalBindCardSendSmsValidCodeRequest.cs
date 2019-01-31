using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Domain.SettleDomain.Bill99.v1_0
{
    /// <summary>
    /// 
    /// </summary>
    public class RawPersonalBindCardSendSmsValidCodeRequestV1
    {
        /// <summary>
        /// 
        /// </summary>
        public String requestId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public String uId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public String platformCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public String identitycardId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public String name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public String phonNumber { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public String requestTime { get; set; }
    }
}
