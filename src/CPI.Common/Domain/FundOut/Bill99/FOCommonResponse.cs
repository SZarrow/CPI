using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Domain.FundOut.Bill99
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class FOCommonResponse
    {
        /// <summary>
        /// 
        /// </summary>
        protected FOCommonResponse() { }

        /// <summary>
        /// 会员编号
        /// </summary>
        public String MemberCode { get; set; }
        /// <summary>
        /// 错误码
        /// </summary>
        public String ErrorCode { get; set; }
        /// <summary>
        /// 错误描述
        /// </summary>
        public String ErrorMessage { get; set; }
    }
}
