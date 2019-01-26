using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Domain.SettleDomain.Bill99
{
    /// <summary>
    /// 个人账户注册响应类
    /// </summary>
    public class PersonalRegisterResponse : CommonResponse
    {
        /// <summary>
        /// 快钱测用户Id
        /// </summary>
        public String OpenId { get; set; }
    }
}
