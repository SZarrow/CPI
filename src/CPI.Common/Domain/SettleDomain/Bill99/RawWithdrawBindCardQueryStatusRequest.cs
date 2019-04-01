using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using ATBase.Validation;
using Newtonsoft.Json;

namespace CPI.Common.Domain.SettleDomain.Bill99
{
    /// <summary>
    /// 提现绑卡状态查询
    /// </summary>
    public class RawWithdrawBindCardQueryStatusRequest
    {
        /// <summary>
        /// 平台用户ID
        /// </summary>
        public String uId { get; set; }

        /// <summary>
        /// 银行卡号
        /// </summary>
        public String bankAcctId { get; set; }
    }
}
