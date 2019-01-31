using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace CPI.Common.Domain.SettleDomain.Bill99.v1_0
{
    /// <summary>
    /// 个人账户提现绑卡响应类
    /// </summary>
    public class PersonalWithdrawBindCardResponseV1 : CommonResponse
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
