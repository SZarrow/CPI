using System;
using System.Collections.Generic;
using System.Text;
using CPI.Common.Models;
using Newtonsoft.Json;

namespace CPI.Common.Domain.SettleDomain.Bill99
{
    /// <summary>
    /// 个人账户已绑定银行卡列表查询响应类
    /// </summary>
    public class PersonalBoundCardListQueryResponse
    {
        /// <summary>
        /// 提现绑卡列表
        /// </summary>
        public IEnumerable<WithdrawBankCardBindInfo> BindCards { get; set; }
    }
}
