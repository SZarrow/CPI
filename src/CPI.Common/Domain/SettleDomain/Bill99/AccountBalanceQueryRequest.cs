using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using ATBase.Validation;
using Newtonsoft.Json;

namespace CPI.Common.Domain.SettleDomain.Bill99
{
    /// <summary>
    /// 账户余额查询请求类
    /// </summary>
    public class AccountBalanceQueryRequest : ValidateModel
    {
        /// <summary>
        /// 分配给接入用户的Id
        /// </summary>
        [Required(ErrorMessage ="AppId字段必需")]
        public String AppId { get; set; }

        /// <summary>
        /// 收款人Id
        /// </summary>
        [Required(ErrorMessage = "PayeeId字段必需")]
        public String PayeeId { get; set; }

        /// <summary>
        /// 账户余额类型，SPAD0001：待分账账户，SPAW0001：可提现账户
        /// </summary>
        public IEnumerable<String> AccountBalanceTypes { get; set; }
    }
}
