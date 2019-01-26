using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace CPI.Common
{
    /// <summary>
    /// 通用状态类
    /// </summary>
    public enum CommonStatus
    {
        /// <summary>
        /// 成功
        /// </summary>
        [Description("成功")]
        SUCCESS,
        /// <summary>
        /// 失败
        /// </summary>
        [Description("失败")]
        FAILURE
    }
}
