using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using ATBase.Validation;
using Newtonsoft.Json;

namespace CPI.Common.Domain.SettleDomain.Bill99
{
    /// <summary>
    /// 个人账户取消绑卡请求类
    /// </summary>
    public class PersonalCancelBoundCardRequest : ValidateModel
    {
        /// <summary>
        /// 分配给接入平台的Id
        /// </summary>
        [JsonIgnore]
        [Required(ErrorMessage = "AppId字段必需")]
        public String AppId { get; set; }

        /// <summary>
        /// 收款人Id
        /// </summary>
        [JsonProperty("uId")]
        [Required(ErrorMessage = "UID字段必需")]
        public String PayeeId { get; set; }
    }
}
