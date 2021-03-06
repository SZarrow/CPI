﻿using System;
using System.Collections.Generic;
using System.Text;
using CPI.Common.Domain.AgreePay;
using ATBase.Core;
using ATBase.Core.Collections;

namespace CPI.IService.AgreePay
{
    /// <summary>
    /// 协议支付服务接口
    /// </summary>
    public interface IAgreementPaymentService
    {
        /// <summary>
        /// 申请支付请求
        /// </summary>
        /// <param name="request">申请请求参数</param>
        XResult<CPIAgreePayApplyResponse> Apply(CPIAgreePayApplyRequest request);
        /// <summary>
        /// 绑卡
        /// </summary>
        /// <param name="request">绑卡请求参数</param>
        XResult<CPIAgreePayBindCardResponse> BindCard(CPIAgreePayBindCardRequest request);
        /// <summary>
        /// 支付
        /// </summary>
        /// <param name="request">支付请求参数</param>
        XResult<CPIAgreePayPaymentResponse> Pay(CPIAgreePayPaymentRequest request);
        /// <summary>
        /// 支付单状态查询
        /// </summary>
        /// <param name="request">查询请求参数</param>
        XResult<PagedList<CPIAgreePayQueryResult>> QueryStatus(CPIAgreePayQueryRequest request);
        /// <summary>
        /// 支付单明细查询
        /// </summary>
        /// <param name="request">查询请求参数</param>
        XResult<PagedList<CPIAgreePayDetailQueryResult>> QueryDetail(CPIAgreePayDetailQueryRequest request);
        /// <summary>
        /// 从快钱拉取支付结果
        /// </summary>
        /// <param name="count">拉取的个数</param>
        XResult<Int32> Pull(Int32 count);
    }
}
