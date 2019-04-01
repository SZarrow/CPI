using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using ATBase.Validation;
using Newtonsoft.Json;

namespace CPI.Common.Domain.SettleDomain.Bill99
{
    /// <summary>
    /// 消费分账请求类
    /// </summary>
    public class AllotAmountPayRequest : ValidateModel
    {
        /// <summary>
        /// 分配给接入平台Id
        /// </summary>
        [Required(ErrorMessage = "AppId字段必需")]
        public String AppId { get; set; }

        /// <summary>
        /// 收款人ID
        /// </summary>
        [Required(ErrorMessage = "PayeeId字段必需")]
        public String PayeeId { get; set; }

        /// <summary>
        /// 提现单外部交易编号
        /// </summary>
        [Required(ErrorMessage = "WithdrawOutTradeNo字段必需")]
        public String WithdrawOutTradeNo { get; set; }

        /// <summary>
        /// 分账总金额
        /// </summary>
        [Required(ErrorMessage = "TotalAmount字段必需")]
        public Decimal TotalAmount { get; set; }

        /// <summary>
        /// 结算周期
        /// </summary>
        [Required(ErrorMessage = "SettlePeriod字段必需")]
        public String SettlePeriod { get; set; }

        public override ValidateResult Validate()
        {
            if (this.TotalAmount <= 0)
            {
                return new ValidateResult(false, "总金额必须大于0");
            }

            return base.Validate();
        }

    }
}
