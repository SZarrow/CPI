using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Domain.SettleDomain.Bill99
{
    /// <summary>
    /// 申请分账提现响应类
    /// </summary>
    public class AllotAmountWithdrawApplyResponse : CommonResponse
    {
        /// <summary>
        /// 收款人Id
        /// </summary>
        public String PayeeId { get; set; }
        /// <summary>
        /// 外部交易编号
        /// </summary>
        public String OutTradeNo { get; set; }
        /// <summary>
        /// 提现金额
        /// </summary>
        public Decimal Amount { get; set; }
    }
}
