using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Lotus.Validation;
using Newtonsoft.Json;

namespace CPI.Common.Domain.SettleDomain.Bill99
{
    /// <summary>
    /// 个人账户开户请求类
    /// </summary>
    public class PersonalRegisterRequest : ValidateModel
    {
        /// <summary>
        /// 分配给接入平台的Id
        /// </summary>
        [Required(ErrorMessage = "AppId字段必需")]
        public String AppId { get; set; }

        /// <summary>
        /// 收款人Id
        /// </summary>
        [Required(ErrorMessage = "PayeeId字段必需")]
        [StringLength(128, ErrorMessage = "PayeeId字段最大长度为128")]
        public String PayeeId { get; set; }

        /// <summary>
        /// 证件类型，身份证：101
        /// </summary>
        [Required(ErrorMessage = "IDCardType字段必需")]
        [StringLength(5, ErrorMessage = "IDCardType字段最大长度为5")]
        public String IDCardType { get; set; }

        /// <summary>
        /// 证件号码
        /// </summary>
        [Required(ErrorMessage = "IDCardNo字段必需")]
        [StringLength(30, ErrorMessage = "IDCardNo字段最大长度为30")]
        public String IDCardNo { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        [Required(ErrorMessage = "RealName字段必需")]
        [StringLength(50, ErrorMessage = "RealName字段最大长度为50")]
        public String RealName { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        [Required(ErrorMessage = "Mobile字段必需")]
        [StringLength(20, ErrorMessage = "Mobile最大长度为20")]
        public String Mobile { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        [Required(ErrorMessage = "Email字段必需")]
        [StringLength(128, ErrorMessage = "Email最大长度为128")]
        public String Email { get; set; }
    }
}
