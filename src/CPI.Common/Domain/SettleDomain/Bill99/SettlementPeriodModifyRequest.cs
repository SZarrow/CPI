using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Lotus.Validation;
using Newtonsoft.Json;

namespace CPI.Common.Domain.SettleDomain.Bill99
{
    /// <summary>
    /// 修改分账周期请求类
    /// </summary>
    public class SettlementPeriodModifyRequest : ValidateModel
    {
        /// <summary>
        /// 分配给接入平台的Id
        /// </summary>
        [JsonIgnore]
        [Required(ErrorMessage = "AppId字段必需")]
        public String AppId { get; set; }

        /// <summary>
        /// 外部订单号
        /// </summary>
        [JsonProperty("outOrderNo")]
        [Required(ErrorMessage = "OutOrderNo字段必需")]
        public String OutOrderNo { get; set; }

        /// <summary>
        /// 外部子订单号
        /// </summary>
        [JsonProperty("outSubOrderNo")]
        [Required(ErrorMessage = "OutSubOrderNo字段必需")]
        public String OutSubOrderNo { get; set; }

        /// <summary>
        /// 结算周期
        /// </summary>
        [JsonProperty("settlePeriod")]
        [Required(ErrorMessage = "SettlePeriod字段必需")]
        [RegularExpression(@"^[TtDd]\+\d{1,2}$", ErrorMessage = "结算周期格式错误")]
        public String SettlePeriod { get; set; }
    }
}
