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
    public class PersonalBindCardSendSmsValidCodeRequestV1 : ValidateModel
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
        [Required(ErrorMessage = "IDCardNo字段必需")]
        public String IDCardNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Required(ErrorMessage = "RealName字段必需")]
        public String RealName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Required(ErrorMessage = "Mobile字段必需")]
        public String Mobile { get; set; }
    }
}
