using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace CPI.Common.Domain.AgreePay
{
    /// <summary>
    /// 申请支付返回结果类
    /// </summary>
    public class CPIAgreePayApplyResponse : CommonResponse
    {
        /// <summary>
        /// 付款人Id
        /// </summary>
        public String PayerId { get; set; }

        /// <summary>
        /// 外部交易号（商户订单号）
        /// </summary>
        public String OutTradeNo { get; set; }

        /// <summary>
        /// 申请的令牌，绑卡操作需要此令牌
        /// </summary>
        public String ApplyToken { get; set; }

        /// <summary>
        /// 申请时间
        /// </summary>
        public String ApplyTime { get; set; }
    }
}
