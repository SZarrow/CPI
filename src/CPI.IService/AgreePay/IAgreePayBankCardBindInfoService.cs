using CPI.Common.Domain.AgreePay;
using CPI.Common.Models;
using Lotus.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.IService.AgreePay
{
    /// <summary>
    /// 协议支付绑卡信息服务接口
    /// </summary>
    public interface IAgreePayBankCardBindInfoService
    {
        /// <summary>
        /// 根据外部跟踪编号获取绑卡信息
        /// </summary>
        /// <param name="payerId">付款人ID</param>
        /// <param name="bankCardNo">银行卡号</param>
        /// <param name="payChannelCode">支付通道编码</param>
        XResult<IEnumerable<AgreePayBankCardBindDetail>> GetBankCardBindDetails(String payerId, String bankCardNo, String payChannelCode);
    }
}
