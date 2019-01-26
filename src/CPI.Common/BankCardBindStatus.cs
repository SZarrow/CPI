using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace CPI.Common
{
    /// <summary>
    /// 银行卡绑定状态
    /// </summary>
    public enum BankCardBindStatus
    {
        /// <summary>
        /// 未绑定
        /// </summary>
        [Description("未绑定")]
        UNBOUND,
        /// <summary>
        /// 绑定
        /// </summary>
        [Description("绑定")]
        BOUND
    }
}
