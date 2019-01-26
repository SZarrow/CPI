using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Lotus.Validation;
using Newtonsoft.Json;

namespace CPI.Common.Domain.SettleDomain.Bill99
{
    /// <summary>
    /// 通用提现请求类
    /// </summary>
    public class CommonWithdrawRequest : ValidateModel
    {
        /// <summary>
        /// 收款人Id
        /// </summary>
        [Required(ErrorMessage = "PayeeId字段必需")]
        public String PayeeId { get; set; }
        /// <summary>
        /// 外部交易编号
        /// </summary>
        [Required(ErrorMessage = "OutTradeNo字段必需")]
        public String OutTradeNo { get; set; }
        /// <summary>
        /// 提现金额
        /// </summary>
        [Required(ErrorMessage = "Amount字段必需")]
        [JsonConverter(typeof(AmountToCentJsonConverter))]
        public Decimal Amount { get; set; }
        /// <summary>
        /// 结算周期，T+0或D+0
        /// </summary>
        [Required(ErrorMessage = "SettlePeriod字段必需")]
        [RegularExpression(@"^(T|D)\+0$", ErrorMessage = "SettlePeriod字段格式错误")]
        public String SettlePeriod { get; set; }
    }
}
