using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Domain.SettleDomain.Bill99
{
    /// <summary>
    /// 个人提现绑卡状态查询响应类
    /// </summary>
    public class PersonalBindCardStatusQueryResponse : CommonResponse
    {
        /// <summary>
        /// 绑卡列表
        /// </summary>
        public IEnumerable<WithdrawBindCardInfo> BindCards { get; set; }
    }
}
