using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace CPI.Common
{
    /// <summary>
    /// 分账状态
    /// </summary>
    public enum AllotAmountOrderStatus
    {
        /// <summary>
        /// 申请中
        /// </summary>
        [Description("分账申请中")]
        APPLY,
        /// <summary>
        /// 处理中
        /// </summary>
        [Description("分账处理中")]
        PROCESSING,
        /// <summary>
        /// 成功
        /// </summary>
        [Description("分账成功")]
        SUCCESS,
        /// <summary>
        /// 失败
        /// </summary>
        [Description("分账失败")]
        FAILURE
    }
}
