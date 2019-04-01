using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using ATBase.Validation;
using Newtonsoft.Json;

namespace CPI.Common.Domain.SettleDomain.Bill99
{
    /// <summary>
    /// 分账明细查询请求类
    /// </summary>
    public class AllotAmountResultQueryRequest : ValidateModel
    {
        /// <summary>
        /// 分配给接入平台的Id
        /// </summary>
        [JsonIgnore]
        [Required(ErrorMessage = "AppId")]
        public String AppId { get; set; }

        /// <summary>
        /// 消费分账外部订单编号或退货分账的外部订单编号
        /// </summary>
        [JsonProperty("outOrderNo")]
        [Required(ErrorMessage = "OutOrderNo字段必需")]
        public String OutOrderNo { get; set; }
    }
}
