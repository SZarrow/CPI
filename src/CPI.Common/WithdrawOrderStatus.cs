using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace CPI.Common
{
    /// <summary>
    /// 提现订单状态
    /// </summary>
    public enum WithdrawOrderStatus
    {
        /// <summary>
        /// 申请提现
        /// </summary>
        [Description("申请中")]
        APPLY,
        /// <summary>
        /// 处理中
        /// </summary>
        [Description("处理中")]
        PROCESSING,
        /// <summary>
        /// 提现成功
        /// </summary>
        [Description("成功")]
        SUCCESS,
        /// <summary>
        /// 提现失败
        /// </summary>
        [Description("失败")]
        FAILURE
    }
}
