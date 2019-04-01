using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using ATBase.Validation;
using Newtonsoft.Json;

namespace CPI.Common.Domain.SettleDomain.Bill99
{
    /// <summary>
    /// 个人账户信息更新请求类
    /// </summary>
    public class PersonalInfoUpdateRequest : ValidateModel
    {
        /// <summary>
        /// 分配给接入平台的Id
        /// </summary>
        [JsonIgnore]
        [Required(ErrorMessage = "AppId字段必需")]
        public String AppId { get; set; }

        /// <summary>
        /// 荷花平台用户Id，必填
        /// </summary>
        [JsonProperty("uId")]
        [Required(ErrorMessage = "UID字段必需")]
        public String UID { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        [JsonProperty("mobile")]
        public String Mobile { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        [JsonProperty("email")]
        public String Email { get; set; }
    }
}
