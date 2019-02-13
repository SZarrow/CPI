using System;
using System.ComponentModel.DataAnnotations;
using Lotus.Validation;

namespace CPI.Common.Domain.SettleDomain.Bill99.v1_0
{
    public class PersonalWithdrawRequestV1 : ValidateModel
    {
        [Required(ErrorMessage = "AppId字段必需")]
        public String AppId { get; set; }

        /// <summary>
        /// 交易功能代码
        /// </summary>
        [Required(ErrorMessage = "FunctionCode字段必需")]
        public String FunctionCode { get; set; }

        /// <summary>
        /// 外部交易编号
        /// </summary>
        [Required(ErrorMessage = "OutTradeNo字段必需")]
        public String OutTradeNo { get; set; }

        /// <summary>
        /// 平台商户名称
        /// </summary>
        [Required(ErrorMessage = "MerchantName字段必需")]
        public String MerchantName { get; set; }

        /// <summary>
        /// 收款人Id
        /// </summary>
        [Required(ErrorMessage = "PayeeId字段必需")]
        public String PayeeId { get; set; }

        /// <summary>
        /// 商户是否是平台，0：是，1：否。
        /// </summary>
        [Required(ErrorMessage = "IsPlatformMerchant字段必需")]
        public String IsPlatformMerchant { get; set; }

        /// <summary>
        /// 收款人真实姓名
        /// </summary>
        [Required(ErrorMessage = "PayeeRealName字段必需")]
        public String PayeeRealName { get; set; }

        /// <summary>
        /// 收款金额
        /// </summary>
        [Required(ErrorMessage = "Amount字段必需")]
        public Decimal Amount { get; set; }

        /// <summary>
        /// 收款银行卡号
        /// </summary>
        [Required(ErrorMessage = "BankCardNo字段必需")]
        public String BankCardNo { get; set; }

        /// <summary>
        /// 收款银行名称
        /// </summary>
        [Required(ErrorMessage = "BankName字段必需")]
        public String BankName { get; set; }

        /// <summary>
        /// 支付方式，10：支付账户余额
        /// </summary>
        [Required(ErrorMessage = "PayMode字段必需")]
        public String PayMode { get; set; }

        /// <summary>
        /// 订单类型，260001：支付账户提现
        /// </summary>
        [Required(ErrorMessage = "OrderType字段必需")]
        public String OrderType { get; set; }
    }
}
