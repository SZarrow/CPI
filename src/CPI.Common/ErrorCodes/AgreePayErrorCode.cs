using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace CPI.Common.ErrorCodes
{
    /// <summary>
    /// 协议支付错误码，取值范围：[2001,3000]
    /// </summary>
    public sealed class AgreePayErrorCode
    {
        /// <summary>
        /// 未绑卡
        /// </summary>
        [Description("未绑卡")]
        public const Int32 NO_BANKCARD_BOUND = 2001;
    }
}
