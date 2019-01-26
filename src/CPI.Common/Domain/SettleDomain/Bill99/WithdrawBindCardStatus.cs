using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace CPI.Common.Domain.SettleDomain.Bill99
{
    /// <summary>
    /// 提现绑卡状态
    /// </summary>
    public enum WithdrawBindCardStatus
    {
        /// <summary>
        /// 处理中
        /// </summary>
        [Description("处理中")]
        PROCESSING,
        /// <summary>
        /// 待审核
        /// </summary>
        [Description("待审核")]
        WAITFORAUDIT,
        /// <summary>
        /// 成功
        /// </summary>
        [Description("绑卡成功")]
        SUCCESS,
        /// <summary>
        /// 失败
        /// </summary>
        [Description("绑卡失败")]
        FAILURE
    }
}
