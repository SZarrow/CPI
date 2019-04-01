using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using ATBase.Validation;
using Newtonsoft.Json;

namespace CPI.Common.Domain.SettleDomain.Bill99
{
    /// <summary>
    /// 提现手续费查询请求类
    /// </summary>
    public class RawWithdrawQueryFeeRequest
    {
        /// <summary>
        /// 平台用户ID
        /// </summary>
        public String uId { get; set; }

        /// <summary>
        /// 提现金额
        /// </summary>
        [JsonConverter(typeof(AmountToCentJsonConverter))]
        public Decimal amount { get; set; }
    }
}
