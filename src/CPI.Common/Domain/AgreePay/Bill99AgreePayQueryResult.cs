using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Domain.AgreePay
{
    /// <summary>
    /// 
    /// </summary>
    public class Bill99AgreePayQueryResult
    {
        /// <summary>
        /// 外部交易编号
        /// </summary>
        public String OutTradeNo { get; set; }
        /// <summary>
        /// 支付状态
        /// </summary>
        public PayStatus PayStatus { get; set; }
    }
}
