using System;
using System.Collections.Generic;
using System.Text;
using CPI.Common.Domain.FundOut.EPay95;
using ATBase.Core;
using ATBase.Core.Collections;

namespace CPI.IService.FundOut
{
    /// <summary>
    /// 双乾单笔代付服务接口
    /// </summary>
    public interface IEPay95SinglePaymentService
    {
        /// <summary>
        /// 代付
        /// </summary>
        /// <param name="request">请求参数</param>
        XResult<PayResponse> Pay(PayRequest request);
        /// <summary>
        /// 更新代付结果
        /// </summary>
        /// <param name="result">请求参数</param>
        XResult<Boolean> UpdatePayStatus(PayNotifyResult result);
        /// <summary>
        /// 查询代付状态
        /// </summary>
        /// <param name="request">请求参数</param>
        XResult<PagedList<QueryStatusResult>> QueryStatus(QueryRequest request);
        /// <summary>
        /// 查询代付结果明细
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        XResult<PagedList<QueryDetailResult>> QueryDetails(QueryRequest request);
    }
}
