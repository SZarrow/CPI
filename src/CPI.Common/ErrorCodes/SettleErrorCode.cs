using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace CPI.Common
{
    /// <summary>
    /// 分账提现错误码类，取值范围：[6001-7000]
    /// </summary>
    public sealed class SettleErrorCode
    {
        /// <summary>
        /// 未开户
        /// </summary>
        [Description("未开户")]
        public const Int32 UN_REGISTERED = 6001;
        /// <summary>
        /// 未绑卡
        /// </summary>
        [Description("未绑卡")]
        public const Int32 NO_BANKCARD_BOUND = 6002;
    }
}
