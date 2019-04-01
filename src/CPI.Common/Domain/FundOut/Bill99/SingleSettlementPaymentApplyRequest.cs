using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using ATBase.Serialization;
using ATBase.Validation;

namespace CPI.Common.Domain.FundOut.Bill99
{
    /// <summary>
    /// 单笔代付申请请求类
    /// </summary>
    [XElement("pay2BankOrder")]
    public class SingleSettlementPaymentApplyRequest : ValidateModel, IPaymentRequest
    {
        /// <summary>
        /// 分配给接入平台的Id
        /// </summary>
        [Required(ErrorMessage = "AppId字段必需")]
        public String AppId { get; set; }

        /// <summary>
        /// 商家订单号
        /// </summary>
        [XElement("orderId")]
        [Required(ErrorMessage = "OrderNo字段必需")]
        public String OrderNo { get; set; }

        /// <summary>
        /// 付款金额，单位：元
        /// </summary>
        [XElement("amount")]
        [Required(ErrorMessage = "Amount字段必需")]
        public Decimal Amount { get; set; }
        /// <summary>
        /// 收款人姓名
        /// </summary>
        [XElement("creditName")]
        [Required(ErrorMessage = "CreditName字段必需")]
        public String CreditName { get; set; }
        /// <summary>
        /// 银行名称
        /// </summary>
        [XElement("bankName")]
        [Required(ErrorMessage = "BankName字段必需")]
        public String BankName { get; set; }
        /// <summary>
        /// 银行卡号
        /// </summary>
        [XElement("bankAcctId")]
        [Required(ErrorMessage = "BankCardNo字段必需")]
        public String BankCardNo { get; set; }
        /// <summary>
        /// 付费方式，0表示收款方付款，1表示付款方付费，默认是1
        /// </summary>
        [XElement("feeAction")]
        [Required(ErrorMessage = "FeeAction字段必需")]
        public String FeeAction { get; set; } = "1";
        /// <summary>
        /// 收款人手机号，可不填，填了会发短信
        /// </summary>
        [XElement("mobile")]
        public String Mobile { get; set; }
        /// <summary>
        /// 备注，可不填
        /// </summary>
        [XElement("remark")]
        public String Remark { get; set; }

        /// <summary>
        /// 获取支付金额
        /// </summary>
        public Decimal GetPayAmount()
        {
            return this.Amount;
        }
    }
}
