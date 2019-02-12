using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Lotus.Validation;
using Newtonsoft.Json;

namespace CPI.Common.Domain.AgreePay
{
    /// <summary>
    /// 支付请求参数类
    /// </summary>
    public class CPIAgreePayPaymentRequest : ValidateModel, IPaymentRequest
    {
        /// <summary>
        /// 分配给接入平台的Id
        /// </summary>
        [Required(ErrorMessage = "AppId字段必需")]
        public String AppId { get; set; }

        /// <summary>
        /// 付款人Id
        /// </summary>
        [Required(ErrorMessage = "PayerId字段必需")]
        public String PayerId { get; set; }

        /// <summary>
        /// 交易金额
        /// </summary>
        [Required(ErrorMessage = "Amount字段必需")]
        public Decimal Amount { get; set; }

        /// <summary>
        /// 分账类型
        /// </summary>
        [Required(ErrorMessage = "SharingType字段必需")]
        [RegularExpression("^0|1$", ErrorMessage = "SharingType字段格式错误")]
        public String SharingType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Required(ErrorMessage = "FeePayerId字段必需")]
        public String FeePayerId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Required(ErrorMessage = "Fee字段必需")]
        public Decimal Fee { get; set; }

        /// <summary>
        /// 分账周期
        /// </summary>
        [Required(ErrorMessage = "SharingPeriod字段必需")]
        [RegularExpression(@"^T\+\d$", ErrorMessage = "SharingPeriod字段格式错误")]
        public String SharingPeriod { get; set; }

        /// <summary>
        /// 支付外部交易号
        /// </summary>
        [Required(ErrorMessage = "OutTradeNo字段必需")]
        public String OutTradeNo { get; set; }

        /// <summary>
        /// 支付令牌
        /// </summary>
        [Required(ErrorMessage = "PayToken字段必需")]
        public String PayToken { get; set; }

        /// <summary>
        /// 银行预留手机号
        /// </summary>
        [Required(ErrorMessage = "Mobile字段必需")]
        public String Mobile { get; set; }

        /// <summary>
        /// 银行卡号
        /// </summary>
        [Required(ErrorMessage = "BankCardNo字段必需")]
        public String BankCardNo { get; set; }

        /// <summary>
        /// 通知地址
        /// </summary>
        public String NotifyUrl { get; set; }

        /// <summary>
        /// 获取支付金额
        /// </summary>
        public Decimal GetPayAmount()
        {
            return this.Amount;
        }
    }
}
