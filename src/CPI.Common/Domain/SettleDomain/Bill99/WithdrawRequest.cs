using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Lotus.Validation;
using Newtonsoft.Json;

namespace CPI.Common.Domain.SettleDomain.Bill99
{
    /// <summary>
    /// 账户提现请求类
    /// </summary>
    public class WithdrawRequest : ValidateModel
    {
        /// <summary>
        /// 分配给接入平台的Id
        /// </summary>
        [Required(ErrorMessage = "AppId字段必需")]
        public String AppId { get; set; }

        /// <summary>
        /// 平台用户ID
        /// </summary>
        [Required(ErrorMessage = "PayeeId字段必需")]
        public String PayeeId { get; set; }

        /// <summary>
        /// 外部交易号
        /// </summary>
        [Required(ErrorMessage = "OutTradeNo字段必需")]
        public String OutTradeNo { get; set; }

        /// <summary>
        /// 提现金额
        /// </summary>
        public Decimal Amount { get; set; }

        /// <summary>
        /// 客户自付手续费
        /// </summary>
        public Decimal CustomerFee { get; set; } = 0;

        /// <summary>
        /// 商户代付手续费
        /// </summary>
        public Decimal MerchantFee { get; set; } = 0;

        /// <summary>
        /// 结算周期
        /// </summary>
        [Required(ErrorMessage = "SettlePeriod字段必需")]
        [RegularExpression(@"^(T|D)\+0$", ErrorMessage = "SettlePeriod字段格式错误")]
        public String SettlePeriod { get; set; }
    }
}
