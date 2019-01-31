using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Domain.SettleDomain.Bill99.v1_0
{
    /// <summary>
    /// 原始个人账户提现绑卡请求类
    /// </summary>
    public class RawPersonalWithdrawBindCardRequestV1
    {
        /// <summary>
        /// 
        /// </summary>
        public String requestId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public String platformCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public String uId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public String token { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public String bankName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public String bankAcctId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public String mobile { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public String idCardNumber { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public String idCardType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public String name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public String validCode { get; set; }
    }
}
