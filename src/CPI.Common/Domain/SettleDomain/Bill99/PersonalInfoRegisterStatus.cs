using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace CPI.Common.Domain.SettleDomain.Bill99
{
    /// <summary>
    /// 个人账户信息审核状态
    /// </summary>
    public enum PersonalInfoRegisterStatus
    {
        /// <summary>
        /// 待审核
        /// </summary>
        [Description("待审核")]
        WAITFORAUDIT,
        /// <summary>
        /// 待复审
        /// </summary>
        [Description("待复审")]
        WAITFORREVIEW,
        /// <summary>
        /// 开户成功
        /// </summary>
        [Description("开户成功")]
        SUCCESS
    }
}
