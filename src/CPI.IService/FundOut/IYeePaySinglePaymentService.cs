using System;
using System.Collections.Generic;
using System.Text;
using CPI.Common.Domain.FundOut.YeePay;
using ATBase.Core;

namespace CPI.IService.FundOut
{
    /// <summary>
    /// 易宝单笔代付服务接口
    /// </summary>
    public interface IYeePaySinglePaymentService
    {
        /// <summary>
        /// 单笔代付
        /// </summary>
        /// <param name="request"></param>
        XResult<YeePaySinglePayResponse> Pay(YeePaySinglePayRequest request);
        /// <summary>
        /// 查询代付结果状态
        /// </summary>
        /// <param name="request"></param>
        XResult<YeePaySinglePayResultQueryResponse> QueryStatus(YeePaySinglePayResultQueryRequest request);
    }
}
