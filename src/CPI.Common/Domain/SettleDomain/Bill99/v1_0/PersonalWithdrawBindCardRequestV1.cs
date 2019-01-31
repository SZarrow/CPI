using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Lotus.Validation;
using Newtonsoft.Json;

namespace CPI.Common.Domain.SettleDomain.Bill99.v1_0
{
    /// <summary>
    /// 个人账户提现绑卡请求类
    /// </summary>
    public class PersonalWithdrawBindCardRequestV1 : ValidateModel
    {
        /// <summary>
        /// 分配给接入平台Id，必填
        /// </summary>
        [Required(ErrorMessage = "AppId字段必需")]
        public String AppId { get; set; }

        /// <summary>
        /// 收款人Id，必填
        /// </summary>
        [Required(ErrorMessage = "UserId字段必需")]
        public String UserId { get; set; }

        /// <summary>
        /// 外部交易编号，必填
        /// </summary>
        [Required(ErrorMessage = "OutTradeNo字段必需")]
        public String OutTradeNo { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Required(ErrorMessage = "ApplyToken字段必需")]
        public String ApplyToken { get; set; }

        /// <summary>
        /// 短信验证码，必填
        /// </summary>
        [Required(ErrorMessage = "SmsValidCode字段必需")]
        public String SmsValidCode { get; set; }

        /// <summary>
        /// 银行名称，必填
        /// </summary>
        [Required(ErrorMessage = "BankName字段必需")]
        public String BankName { get; set; }

        /// <summary>
        /// 银行卡号，必填
        /// </summary>
        [Required(ErrorMessage = "BankCardNo字段必需")]
        public String BankCardNo { get; set; }

        /// <summary>
        /// 证件号码，必填
        /// </summary>
        [Required(ErrorMessage = "IDCardNo字段必需")]
        public String IDCardNo { get; set; }

        /// <summary>
        /// 证件类型，必填
        /// </summary>
        [Required(ErrorMessage = "IDCardType字段必需")]
        public String IDCardType { get; set; }

        /// <summary>
        /// 银行预留手机号，必填
        /// </summary>
        [Required(ErrorMessage = "Mobile字段必需")]
        public String Mobile { get; set; }

        /// <summary>
        /// 真实姓名，必填
        /// </summary>
        [Required(ErrorMessage = "RealName字段必需")]
        public String RealName { get; set; }
    }
}
