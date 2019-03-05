using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Domain.AgreePay.YeePay
{
    public class YeePayAgreePayBindCardResponse : CommonResponse
    {
        /// <summary>
        /// 付款人Id
        /// </summary>
        public String PayerId { get; set; }

        /// <summary>
        /// 外部交易编号
        /// </summary>
        public String OutTradeNo { get; set; }

        /// <summary>
        /// 绑定时间
        /// </summary>
        public String BindTime { get; set; }
    }
}
