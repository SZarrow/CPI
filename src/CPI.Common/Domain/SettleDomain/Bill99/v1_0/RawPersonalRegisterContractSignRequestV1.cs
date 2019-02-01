using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Domain.SettleDomain.Bill99.v1_0
{
    /// <summary>
    /// 
    /// </summary>
    public class RawPersonalRegisterContractSignRequestV1
    {
        /// <summary>
        /// 
        /// </summary>
        public String requestId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public String platformCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public String applyId { get; set; }
        /// <summary>
        /// 签约类型，0：企业，1：个人
        /// </summary>
        public String signType { get; set; }
    }
}
