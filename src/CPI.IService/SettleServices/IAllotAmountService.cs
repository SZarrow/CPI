using System;
using System.Collections.Generic;
using System.Text;
using CPI.Common.Domain.SettleDomain.Bill99;
using Lotus.Core;

namespace CPI.IService.SettleServices
{
    /// <summary>
    /// 分账服务接口
    /// </summary>
    public interface IAllotAmountService
    {
        /// <summary>
        /// 消费分账
        /// </summary>
        /// <param name="request">分账请求参数</param>
        XResult<AllotAmountPayResponse> Pay(AllotAmountPayRequest request);
        /// <summary>
        /// 退货分账
        /// </summary>
        /// <param name="request">退货请求参数</param>
        XResult<AllotAmountRefundResponse> Refund(AllotAmountRefundRequest request);
        /// <summary>
        /// 查询分账结果
        /// </summary>
        /// <param name="request">查询请求参数</param>
        XResult<AllotAmountResultQueryResponse> Query(AllotAmountResultQueryRequest request);
        /// <summary>
        /// 修改结算周期
        /// </summary>
        /// <param name="request">请求参数</param>
        XResult<SettlementPeriodModifyResponse> ModifySettlePeriod(SettlementPeriodModifyRequest request);
    }
}
