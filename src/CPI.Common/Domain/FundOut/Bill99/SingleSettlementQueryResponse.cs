using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Domain.FundOut.Bill99
{
    /// <summary>
    /// 单笔代付查询响应类
    /// </summary>
    public class SingleSettlementQueryResponse : CommonResponse
    {
        /// <summary>
        /// 荷花平台订单编号
        /// </summary>
        public String OutTradeNo { get; set; }

        /// <summary>
        /// CPI内部交易号
        /// </summary>
        public String TradeNo { get; set; }

        /// <summary>
        /// 银行名称
        /// </summary>
        public String BankName { get; set; }

        /// <summary>
        /// 收款人姓名
        /// </summary>
        public String CreditName { get; set; }

        /// <summary>
        /// 收款人银行卡号
        /// </summary>
        public String BankCardNo { get; set; }

        /// <summary>
        /// 收款金额，单位：元
        /// </summary>
        public Decimal Amount { get; set; }

        /// <summary>
        /// 手续费
        /// </summary>
        public Decimal Fee { get; set; }

        /// <summary>
        /// 手续费付费方式，0表示收款方付费，1表示付款方付费
        /// </summary>
        public String FeeAction { get; set; }

        /// <summary>
        /// 订单创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
