using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace CPI.Common.Domain.SettleDomain.Bill99.v1_0
{
    /// <summary>
    /// 原始个人账户注册响应类
    /// </summary>
    public class RawPersonalRegisterResponseV1 : COECommonResponse
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
        public String uId { get; set; }
    }
}
