using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Domain.SettleDomain.Bill99.v1_0
{
    /// <summary>
    /// 个人账户注册响应类
    /// </summary>
    public class PersonalRegisterResponseV1 : CommonResponse
    {
        /// <summary>
        /// 收款人Id
        /// </summary>
        public String PayeeId { get; set; }
    }
}
