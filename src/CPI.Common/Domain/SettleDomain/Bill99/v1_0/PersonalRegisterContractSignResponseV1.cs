using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Domain.SettleDomain.Bill99.v1_0
{
    /// <summary>
    /// 
    /// </summary>
    public class PersonalRegisterContractSignResponseV1 : CommonResponse
    {
        /// <summary>
        /// 
        /// </summary>
        public String UserId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public String SignDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public String StartDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public String EndDate { get; set; }
    }
}
