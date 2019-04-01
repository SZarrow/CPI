using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using ATBase.Validation;

namespace CPI.Common.Domain.AgreePay
{
    /// <summary>
    /// 协议支付通用请求类
    /// </summary>
    public class CommonPayRequest : ValidateModel, IPaymentRequest
    {
        /// <summary>
        /// 付款人ID
        /// </summary>
        [Required(ErrorMessage = "PayerId字段必需")]
        public String PayerId { get; set; }
        /// <summary>
        /// 付款金额
        /// </summary>
        [Required(ErrorMessage = "Amount字段必需")]
        public Decimal Amount { get; set; }
        /// <summary>
        /// 外部交易编号
        /// </summary>
        [Required(ErrorMessage = "OutTradeNo字段必需")]
        public String OutTradeNo { get; set; }
        /// <summary>
        /// 银行卡号
        /// </summary>
        [Required(ErrorMessage = "BankCardNo字段必需")]
        public String BankCardNo { get; set; }

        /// <summary>
        /// 分账信息
        /// </summary>
        public String SharingInfo { get; set; }

        /// <summary>
        /// 通道编号，用于易宝的协议支付和代扣的选择f
        /// </summary>
        public String TerminalNo { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Decimal GetPayAmount()
        {
            return this.Amount;
        }
    }
}
