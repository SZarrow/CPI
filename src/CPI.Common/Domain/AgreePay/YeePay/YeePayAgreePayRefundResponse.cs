using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Domain.AgreePay.YeePay
{
    public class YeePayAgreePayRefundResponse : CommonResponse
    {
        /// <summary>
        /// 外部交易号（商户订单编号）
        /// </summary>
        public String OutTradeNo { get; set; }

        /// <summary>
        /// 易宝内部交易号
        /// </summary>
        public String YeePayTradeNo { get; set; }

        /// <summary>
        /// 退款申请时间
        /// </summary>
        public String ApplyTime { get; set; }
    }
}
