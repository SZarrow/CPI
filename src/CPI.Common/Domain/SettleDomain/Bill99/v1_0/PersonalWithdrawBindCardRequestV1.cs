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
        /// 分配给接入平台Id
        /// </summary>
        [Required(ErrorMessage = "AppId字段必需")]
        public String AppId { get; set; }

        /// <summary>
        /// 收款人Id
        /// </summary>
        [Required(ErrorMessage = "PayeeId字段必需")]
        public String PayeeId { get; set; }

        /// <summary>
        /// 银行卡号，必填
        /// </summary>
        [Required(ErrorMessage = "BankCardNo字段必需")]
        public String BankCardNo { get; set; }

        /// <summary>
        /// 银行预留手机号，必填
        /// </summary>
        [Required(ErrorMessage = "Mobile字段必需")]
        public String Mobile { get; set; }

        /// <summary>
        /// 二类账户标识，0表示非二类账户，1表示是二类账户，默认为0，可选
        /// </summary>
        [RegularExpression(@"^\d$", ErrorMessage = "账户标识格式错误")]
        public String SecondAccountFlag { get; set; } = "0";
    }
}
