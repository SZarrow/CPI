using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Domain.AgreePay.YeePay
{
    public class YeePayAgreePayQueryResult
    {
        /// <summary>
        /// 外部交易编号
        /// </summary>
        public String OutTradeNo { get; set; }
        /// <summary>
        /// 易宝内部交易号
        /// </summary>
        public String YeePayTradeNo { get; set; }
        /// <summary>
        /// 交易金额
        /// </summary>
        public Decimal Amount { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public String CreateTime { get; set; }
        /// <summary>
        /// 完成时间
        /// </summary>
        public String CompleteTime { get; set; }
        /// <summary>
        /// 支付状态
        /// </summary>
        public PayStatus PayStatus { get; set; }
    }
}
