using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace CPI.Common.Domain.SettleDomain.Bill99.v1_0
{
    /// <summary>
    /// 个人开户合同签约状态
    /// </summary>
    public enum PersonalRegisterContractSignStatus
    {
        /// <summary>
        /// 待签约
        /// </summary>
        [Description("待签约")]
        WAIT_FOR_SIGN,
        /// <summary>
        /// 签约成功
        /// </summary>
        [Description("签约成功")]
        SUCCESS,
        /// <summary>
        /// 签约失败
        /// </summary>
        [Description("签约失败")]
        FAILURE,
        /// <summary>
        /// 快钱账户待激活
        /// </summary>
        [Description("快钱账户待激活")]
        WAIT_FOR_ACTIVE,
        /// <summary>
        /// 合同生成，待转PDF
        /// </summary>
        [Description("合同生成，待转PDF")]
        WAIT_FOR_CONVERT_TO_PDF,
        /// <summary>
        /// 签约中
        /// </summary>
        [Description("签约中")]
        SIGNING,
        /// <summary>
        /// 签约拒绝
        /// </summary>
        [Description("签约拒绝")]
        SIGN_REFUSE,
        /// <summary>
        /// 未生成合同
        /// </summary>
        [Description("未生成合同")]
       CONTRACT_NOT_EXIST
    }
}
