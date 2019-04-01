using System;
using System.Collections.Generic;
using System.Text;
using CPI.Common.Domain.FundOut.Bill99;
using ATBase.Core;
using ATBase.Core.Collections;

namespace CPI.IService.FundOut
{
    /// <summary>
    /// 快钱单笔代付接口
    /// </summary>
    public interface IBill99SinglePaymentService
    {
        /// <summary>
        /// 单笔代付
        /// </summary>
        /// <param name="request">单笔代付支付请求参数</param>
        XResult<SingleSettlementPaymentApplyResponse> Pay(SingleSettlementPaymentApplyRequest request);
        /// <summary>
        /// 单笔代付查询
        /// </summary>
        /// <param name="request">单笔代付查询请求参数</param>
        XResult<PagedList<SingleSettlementQueryResponse>> Query(SingleSettlementQueryRequest request);
        /// <summary>
        /// 单笔代付查询订单状态
        /// </summary>
        /// <param name="request">单笔代付查询请求参数</param>
        XResult<PagedList<OrderStatusResult>> QueryStatus(SingleSettlementQueryRequest request);
        /// <summary>
        /// 拉取处理中的订单，然后更新状态
        /// </summary>
        /// <param name="count">拉取的数量，范围[1,20]</param>
        XResult<Int32> Pull(Int32 count);
    }
}
