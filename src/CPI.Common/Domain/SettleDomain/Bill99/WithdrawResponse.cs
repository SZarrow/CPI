using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace CPI.Common.Domain.SettleDomain.Bill99
{
    /// <summary>
    /// 账户提现响应类
    /// </summary>
    public class WithdrawResponse : CommonResponse
    {
        /// <summary>
        /// 盈账通内部交易编号
        /// </summary>
        public String DealId { get; set; }
    }
}
