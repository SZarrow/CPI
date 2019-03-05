using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Domain.AgreePay.YeePay
{
    public class YeePayAgreePayApplyResponse : CommonResponse
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
        /// 申请时间
        /// </summary>
        public String ApplyTime { get; set; }
    }
}
