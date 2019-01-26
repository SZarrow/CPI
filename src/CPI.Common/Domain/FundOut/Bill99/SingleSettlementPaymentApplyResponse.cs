using System;
using System.Collections.Generic;
using System.Text;
using Lotus.Serialization;

namespace CPI.Common.Domain.FundOut.Bill99
{
    /// <summary>
    /// 单笔代付申请响应类
    /// </summary>
    public class SingleSettlementPaymentApplyResponse : FOCommonResponse
    {
        /// <summary>
        /// 订单编号
        /// </summary>
        [XElement("orderId")]
        public String OrderNo { get; set; }

        /// <summary>
        /// 银行卡号
        /// </summary>
        [XElement("bankAcctId")]
        public String BankCardNo { get; set; }

        /// <summary>
        /// 收款金额
        /// </summary>
        [XElement("amount")]
        public Decimal Amount { get; set; }

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
        /// 收款人手机号
        /// </summary>
        [XElement("mobile")]
        public String Mobile { get; set; }

        /// <summary>
        /// 付费方式，0表示收款方付款，1表示付款方付费，默认是1
        /// </summary>
        [XElement("feeAction")]
        public String FeeAction { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [XElement("remark")]
        public String Remark { get; set; }
    }
}
