using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace CPI.Common
{
    /// <summary>
    /// 支付状态
    /// </summary>
    public enum PayStatus
    {
        /// <summary>
        /// 付款申请
        /// </summary>
        [Description("已申请")]
        APPLY,
        /// <summary>
        /// 付款处理中
        /// </summary>
        [Description("处理中")]
        PROCESSING,
        /// <summary>
        /// 付款成功，此状态为最终状态
        /// </summary>
        [Description("成功")]
        SUCCESS,
        /// <summary>
        /// 支付失败， 此状态为最终状态
        /// </summary>
        [Description("失败")]
        FAILURE
    }
}
