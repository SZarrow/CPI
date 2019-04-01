using System;
using System.Collections.Generic;
using System.Text;
using CPI.Common.Domain.AgreePay;
using CPI.Common.Domain.AgreePay.YeePay;
using ATBase.Core;
using ATBase.Core.Collections;

namespace CPI.IService.AgreePay
{
    /// <summary>
    /// 易宝协议支付/代扣
    /// </summary>
    public interface IYeePayAgreementPaymentService
    {
        /// <summary>
        /// 申请绑卡
        /// </summary>
        /// <param name="request">申请请求参数</param>
        XResult<YeePayAgreePayApplyResponse> Apply(YeePayAgreePayApplyRequest request);
        /// <summary>
        /// 支付绑卡
        /// </summary>
        /// <param name="request">绑卡请求参数</param>
        XResult<YeePayAgreePayBindCardResponse> BindCard(YeePayAgreePayBindCardRequest request);
        /// <summary>
        /// 消费支付
        /// </summary>
        /// <param name="request">支付请求参数</param>
        XResult<YeePayAgreePayPaymentResponse> Pay(YeePayAgreePayPaymentRequest request);
        /// <summary>
        /// 申请退款
        /// </summary>
        /// <param name="request"></param>
        XResult<YeePayAgreePayRefundResponse> Refund(YeePayAgreePayRefundRequest request);
        /// <summary>
        /// 从易宝拉取支付结果
        /// </summary>
        /// <param name="count">拉取的个数</param>
        XResult<Int32> PullPayStatus(Int32 count);
        /// <summary>
        /// 从易宝拉取退款结果
        /// </summary>
        /// <param name="count"></param>
        XResult<Int32> PullRefundStatus(Int32 count);
    }
}
