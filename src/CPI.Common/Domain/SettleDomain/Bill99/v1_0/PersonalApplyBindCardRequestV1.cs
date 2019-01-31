using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Lotus.Validation;

namespace CPI.Common.Domain.SettleDomain.Bill99.v1_0
{
    /// <summary>
    /// 
    /// </summary>
    public class PersonalApplyBindCardRequestV1 : ValidateModel
    {
        /// <summary>
        /// 分配给接入平台的Id
        /// </summary>
        [Required(ErrorMessage = "AppId字段必需")]
        public String AppId { get; set; }
        /// <summary>
        /// 外部交易编号
        /// </summary>
        [Required(ErrorMessage = "OutTradeNo字段必需")]
        public String OutTradeNo { get; set; }
        /// <summary>
        /// 用户Id
        /// </summary>
        [Required(ErrorMessage = "UserId字段必需")]
        public String UserId { get; set; }
        /// <summary>
        /// 银行名称
        /// </summary>
        [Required(ErrorMessage = "BankName字段必需")]
        public String BankName { get; set; }
        /// <summary>
        /// 银行卡号
        /// </summary>
        [Required(ErrorMessage = "BankCardNo字段必需")]
        public String BankCardNo { get; set; }
        /// <summary>
        /// 银行预留手机号
        /// </summary>
        [Required(ErrorMessage = "Mobile字段必需")]
        [RegularExpression(@"^\d{11,13}$", ErrorMessage = "手机号格式错误")]
        public String Mobile { get; set; }
        /// <summary>
        /// 证件号码
        /// </summary>
        [Required(ErrorMessage = "IDCardNo字段必需")]
        public String IDCardNo { get; set; }
        /// <summary>
        /// 证件类型，身份证101
        /// </summary>
        [Required(ErrorMessage = "IDCardType字段必需")]
        public String IDCardType { get; set; }
        /// <summary>
        /// 真实姓名
        /// </summary>
        [Required(ErrorMessage = "RealName字段必需")]
        public String RealName { get; set; }
    }
}
