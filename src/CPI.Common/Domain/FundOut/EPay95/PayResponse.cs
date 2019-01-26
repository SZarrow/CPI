using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace CPI.Common.Domain.FundOut.EPay95
{
    /// <summary>
    /// 代付支付响应类
    /// </summary>
    public class PayResponse : CommonResponse
    {
        /// <summary>
        /// 金额
        /// </summary>
        public String Amount { get; set; }
        /// <summary>
        /// 外部交易号
        /// </summary>
        public String OutTradeNo { get; set; }
        /// <summary>
        /// 收款人银行卡号
        /// </summary>
        public String BankCardNo { get; set; }
    }
}
