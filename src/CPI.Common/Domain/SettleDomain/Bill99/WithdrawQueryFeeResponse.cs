using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Domain.SettleDomain.Bill99
{
    /// <summary>
    /// 提现手续费查询响应类
    /// </summary>
    public class WithdrawQueryFeeResponse : CommonResponse
    {
        /// <summary>
        /// 手续费
        /// </summary>
        public Decimal Fee { get; set; }
    }
}
