using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace CPI.Common.Domain.SettleDomain.Bill99
{
    /// <summary>
    /// 快钱返回的提现响应类
    /// </summary>
    public class RawWithdrawResponse : YZTCommonResponse
    {
        /// <summary>
        /// 盈账通内部交易编号
        /// </summary>
        public String dealId { get; set; }

        /// <summary>
        /// 订单状态
        /// </summary>
        public String status { get; set; }
    }
}
