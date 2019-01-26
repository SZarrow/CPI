using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace CPI.Common.Domain.AgreePay
{
    /// <summary>
    /// 绑卡返回参数类
    /// </summary>
    public class CPIAgreePayBindCardResponse : CommonResponse
    {
        /// <summary>
        /// 付款人Id
        /// </summary>
        public String PayerId { get; set; }

        /// <summary>
        /// 支付令牌
        /// </summary>
        public String PayToken { get; set; }

        /// <summary>
        /// 绑定时间
        /// </summary>
        public String BindTime { get; set; }
    }
}
