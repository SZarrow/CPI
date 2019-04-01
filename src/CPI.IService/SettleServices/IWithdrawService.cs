using System;
using System.Collections.Generic;
using System.Text;
using CPI.Common.Domain.SettleDomain.Bill99;
using ATBase.Core;
using ATBase.Core.Collections;

namespace CPI.IService.SettleServices
{
    /// <summary>
    /// 提现服务接口
    /// </summary>
    public interface IWithdrawService
    {
        /// <summary>
        /// 发起提现操作
        /// </summary>
        /// <param name="request">提现请求参数</param>
        XResult<WithdrawResponse> Withdraw(WithdrawRequest request);
        /// <summary>
        /// 查询提现明细
        /// </summary>
        /// <param name="request">查询请求参数</param>
        XResult<WithdrawQueryResponse> QueryDetails(WithdrawQueryRequest request);
        /// <summary>
        /// 查询提现手续费
        /// </summary>
        /// <param name="request">查询请求参数</param>
        XResult<WithdrawQueryFeeResponse> QueryFee(WithdrawQueryFeeRequest request);
        /// <summary>
        /// 查询提现结果状态
        /// </summary>
        /// <param name="request">查询请求参数</param>
        XResult<PagedList<WithdrawStatusQueryResult>> QueryStatus(WithdrawStatusQueryRequest request);
    }
}
