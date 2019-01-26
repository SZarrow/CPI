using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Domain.SettleDomain.Bill99
{
    /// <summary>
    /// 提现状态查询结果类
    /// </summary>
    public class WithdrawStatusQueryResult : CommonResponse
    {
        /// <summary>
        /// 外部交易编号
        /// </summary>
        public String OutTradeNo { get; set; }
        /// <summary>
        /// 提现金额
        /// </summary>
        public Decimal Amount { get; set; }
        /// <summary>
        /// 申请时间
        /// </summary>
        public DateTime ApplyTime { get; set; }
    }
}
