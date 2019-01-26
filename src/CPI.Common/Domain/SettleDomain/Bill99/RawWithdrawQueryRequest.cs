using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Lotus.Validation;
using Newtonsoft.Json;

namespace CPI.Common.Domain.SettleDomain.Bill99
{
    /// <summary>
    /// 账户提现查询请求类
    /// </summary>
    public class RawWithdrawQueryRequest
    {
        /// <summary>
        /// 平台用户ID
        /// </summary>
        public String uId { get; set; }

        /// <summary>
        /// 外部交易编号
        /// </summary>
        public String outTradeNo { get; set; }
    }
}
