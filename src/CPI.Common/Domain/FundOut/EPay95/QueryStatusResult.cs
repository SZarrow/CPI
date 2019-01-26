using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Domain.FundOut.EPay95
{
    /// <summary>
    /// 单笔代付查询状态结果类
    /// </summary>
    public class QueryStatusResult : CommonResponse
    {
        /// <summary>
        /// 外部交易编号
        /// </summary>
        public String OutTradeNo { get; set; }

        /// <summary>
        /// 收款人银行卡号
        /// </summary>
        public String BankCardNo { get; set; }

        /// <summary>
        /// 收款金额，单位：元
        /// </summary>
        public Decimal Amount { get; set; }

        /// <summary>
        /// 订单创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
