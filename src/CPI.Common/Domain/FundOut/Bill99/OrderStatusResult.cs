using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Domain.FundOut.Bill99
{
    /// <summary>
    /// 订单状态结果类
    /// </summary>
    public class OrderStatusResult : CommonResponse
    {
        /// <summary>
        /// 订单编号
        /// </summary>
        public String OutTradeNo { get; set; }
        /// <summary>
        /// 订单创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
