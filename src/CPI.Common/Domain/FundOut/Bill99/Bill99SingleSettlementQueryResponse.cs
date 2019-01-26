using System;
using System.Collections.Generic;
using System.Text;
using Lotus.Serialization;

namespace CPI.Common.Domain.FundOut.Bill99
{
    /// <summary>
    /// 快钱单笔代付查询响应类
    /// </summary>
    public class Bill99SingleSettlementQueryResponse : FOCommonResponse
    {
        /// <summary>
        /// 荷花平台订单编号
        /// </summary>
        [XElement("orderId")]
        public String OrderNo { get; set; }

        /// <summary>
        /// 快钱订单编号
        /// </summary>
        [XElement("orderSeqId")]
        public String Bill99OrderNo { get; set; }

        /// <summary>
        /// 银行名称
        /// </summary>
        [XElement("bankName")]
        public String BankName { get; set; }

        /// <summary>
        /// 收款人姓名
        /// </summary>
        [XElement("creditName")]
        public String CreditName { get; set; }

        /// <summary>
        /// 收款人银行卡号
        /// </summary>
        [XElement("bankAcctId")]
        public String BankCardNo { get; set; }

        /// <summary>
        /// 收款金额，单位：元
        /// </summary>
        [XElement("amount")]
        public Decimal Amount { get; set; }

        /// <summary>
        /// 手续费
        /// </summary>
        [XElement("fee")]
        public Decimal Fee { get; set; }

        /// <summary>
        /// 手续费付费方式，0表示收款方付费，1表示付款方付费
        /// </summary>
        [XElement("feeAction")]
        public String FeeAction { get; set; }

        /// <summary>
        /// 交易状态，100表示申请成功，101表示支付中，111表示支付成功，112表示支付失败，114：已退票
        /// </summary>
        [XElement("status")]
        public String Status { get; set; }

        /// <summary>
        /// 申请时间
        /// </summary>
        [XElement("applyDate")]
        public DateTime ApplyDate { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        [XElement("endDate")]
        public DateTime EndDate { get; set; }
    }
}
