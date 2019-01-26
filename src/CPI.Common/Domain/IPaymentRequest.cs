using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Domain
{
    /// <summary>
    /// 支付请求接口类
    /// </summary>
    public interface IPaymentRequest
    {
        /// <summary>
        /// 获取支付金额
        /// </summary>
        Decimal GetPayAmount();
    }
}
