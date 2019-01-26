using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Lotus.Validation;
using Newtonsoft.Json;

namespace CPI.Common.Domain.SettleDomain.Bill99
{
    /// <summary>
    /// 分账提现请求类
    /// </summary>
    public class AllotAmountWithdrawApplyRequest : ValidateModel
    {
        /// <summary>
        /// 分配给接入平台的Id
        /// </summary>
        [JsonIgnore]
        [Required(ErrorMessage = "AppId字段必需")]
        public String AppId { get; set; }

        /// <summary>
        /// 平台用户ID
        /// </summary>
        [JsonProperty("uId")]
        [Required(ErrorMessage = "uId字段必需")]
        public String PayeeId { get; set; }

        /// <summary>
        /// 外部交易号
        /// </summary>
        [JsonProperty("outTradeNo")]
        [Required(ErrorMessage = "outTradeNo字段必需")]
        public String OutTradeNo { get; set; }

        /// <summary>
        /// 提现金额
        /// </summary>
        [JsonProperty("amount")]
        [JsonConverter(typeof(AmountToCentJsonConverter))]
        public Decimal Amount { get; set; }

        /// <summary>
        /// 客户自付手续费
        /// </summary>
        [JsonProperty("customerFee")]
        [JsonConverter(typeof(AmountToCentJsonConverter))]
        public Decimal CustomerFee { get; set; } = 0;

        /// <summary>
        /// 商户代付手续费
        /// </summary>
        [JsonProperty("merchantFee")]
        [JsonConverter(typeof(AmountToCentJsonConverter))]
        public Decimal MerchantFee { get; set; } = 0;

        /// <summary>
        /// 结算周期
        /// </summary>
        [JsonIgnore]
        [Required(ErrorMessage = "SettlePeriod字段必需")]
        [RegularExpression(@"^(T|D)\+0$", ErrorMessage = "SettlePeriod字段格式错误")]
        public String SettlePeriod { get; set; }
    }
}
