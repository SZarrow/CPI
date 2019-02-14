using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Lotus.Validation;

namespace CPI.Common.Domain.SettleDomain.Bill99.v1_0
{
    public class PersonalAccountBalanceQueryRequestV1 : ValidateModel
    {
        [Required(ErrorMessage = "AppId字段必需")]
        public String AppId { get; set; }

        [Required(ErrorMessage = "UserId字段必需")]
        public String UserId { get; set; }

        /// <summary>
        /// 是否是平台用户，0：否，1：是。
        /// </summary>
        [Required(ErrorMessage = "IsPlatform字段必需")]
        public String IsPlatform { get; set; }

        [Required]
        public String AccountType { get; set; }
    }
}
