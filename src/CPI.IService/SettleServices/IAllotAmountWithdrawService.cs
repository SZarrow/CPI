using System;
using System.Collections.Generic;
using System.Text;
using CPI.Common.Domain.SettleDomain.Bill99;
using Lotus.Core;

namespace CPI.IService.SettleServices
{
    /// <summary>
    /// 提现分账服务接口，调用此接口进行提现会先进行分账。
    /// </summary>
    public interface IAllotAmountWithdrawService
    {
        /// <summary>
        /// 申请提现
        /// </summary>
        /// <param name="request"></param>
        XResult<AllotAmountWithdrawApplyResponse> Apply(AllotAmountWithdrawApplyRequest request);
        /// <summary>
        /// 发起分账
        /// </summary>
        /// <param name="count">每次处理的分账个数</param>
        XResult<Int32> FireAllotAmount(Int32 count = 10);
        /// <summary>
        /// 拉取分账结果
        /// </summary>
        /// <param name="count">每次拉取的个数</param>
        XResult<Int32> PullAllotAmountResult(Int32 count = 20);
        /// <summary>
        /// 发起提现
        /// </summary>
        /// <param name="count">每次处理的提现个数</param>
        XResult<Int32> FireWithdraw(Int32 count = 10);
        /// <summary>
        /// 拉取提现结果
        /// </summary>
        /// <param name="count">每次拉取的个数</param>
        XResult<Int32> PullWithdrawResult(Int32 count = 20);
    }
}
