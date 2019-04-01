using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using ATBase.Validation;

namespace CPI.Common.Domain.SettleDomain.Bill99.v1_0
{
    /// <summary>
    /// 
    /// </summary>
    public class QueryBankCardAcceptRequestV1 : ValidateModel
    {
        /// <summary>
        /// 
        /// </summary>
        [Required(ErrorMessage = "AppId字段必需")]
        public String AppId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Required(ErrorMessage = "UserId字段必需")]
        public String UserId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Required(ErrorMessage = "BankCardNo字段必需")]
        public String BankCardNo { get; set; }
    }
}
