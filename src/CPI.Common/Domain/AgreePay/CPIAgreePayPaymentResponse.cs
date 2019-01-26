using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace CPI.Common.Domain.AgreePay
{
    /// <summary>
    /// 支付响应类
    /// </summary>
    public class CPIAgreePayPaymentResponse : CommonResponse
    {
        /// <summary>
        /// 外部交易号（商户订单编号）
        /// </summary>
        public String OutTradeNo { get; set; }

        /// <summary>
        /// CPI内部交易号
        /// </summary>
        public String TradeNo { get; set; }

        /// <summary>
        /// 支付时间
        /// </summary>
        public String PayTime { get; set; }
    }
}
