using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Domain.AgreePay
{
    /// <summary>
    /// 支付查询结果类
    /// </summary>
    public class CPIAgreePayQueryResult : CommonResponse
    {
        /// <summary>
        /// 外部交易编号
        /// </summary>
        public String OutTradeNo { get; set; }
        /// <summary>
        /// 支付金额
        /// </summary>
        public Decimal Amount { get; set; }
        /// <summary>
        /// 支付类型
        /// </summary>
        public String PayType { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
