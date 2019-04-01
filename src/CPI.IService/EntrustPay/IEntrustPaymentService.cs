using System;
using System.Collections.Generic;
using System.Text;
using CPI.Common.Domain.EntrustPay;
using ATBase.Core;

namespace CPI.IService.EntrustPay
{
    /// <summary>
    /// 委托代收接口
    /// </summary>
    public interface IEntrustPaymentService
    {
        /// <summary>
        /// 支付
        /// </summary>
        /// <param name="request">支付请求参数</param>
        XResult<CPIEntrustPayPaymentResponse> Pay(CPIEntrustPayPaymentRequest request);
        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="request">查询请求参数</param>
        XResult<CPIEntrustPayQueryResponse> Query(CPIEntrustPayQueryRequest request);
    }
}
