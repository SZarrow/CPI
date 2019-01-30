using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Domain.SettleDomain.Bill99.v1_0
{
    /// <summary>
    /// 
    /// </summary>
    public class RawPersonalApplyBindCardRequestV1
    {
        /// <summary>
        /// 外部交易编号
        /// </summary>
        public String requestId { get; set; }
        /// <summary>
        /// 平台编码
        /// </summary>
        public String platformCode { get; set; }
        /// <summary>
        /// 用户Id
        /// </summary>
        public String uId { get; set; }
        /// <summary>
        /// 银行名称
        /// </summary>
        public String bankName { get; set; }
        /// <summary>
        /// 银行卡号
        /// </summary>
        public String bankAcctId { get; set; }
        /// <summary>
        /// 银行预留手机号
        /// </summary>
        public String mobile { get; set; }
        /// <summary>
        /// 证件号码
        /// </summary>
        public String idCardNumber { get; set; }
        /// <summary>
        /// 证件类型，身份证101
        /// </summary>
        public String idCardType { get; set; }
        /// <summary>
        /// 姓名
        /// </summary>
        public String name { get; set; }
    }
}
