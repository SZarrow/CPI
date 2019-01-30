using System;
using System.Collections.Generic;
using System.Text;
using Lotus.Validation;

namespace CPI.Common.Domain.SettleDomain.Bill99.v1_0
{
    /// <summary>
    /// 
    /// </summary>
    public class PersonalApplyBindCardRequestV1 : ValidateModel
    {
        /// <summary>
        /// 分配给接入平台的Id
        /// </summary>
        public String AppId { get; set; }
        /// <summary>
        /// 外部交易编号
        /// </summary>
        public String OutTradeNo { get; set; }
        /// <summary>
        /// 用户Id
        /// </summary>
        public String UserId { get; set; }
        /// <summary>
        /// 银行名称
        /// </summary>
        public String BankName { get; set; }
        /// <summary>
        /// 银行卡号
        /// </summary>
        public String BankCardNo { get; set; }
        /// <summary>
        /// 银行预留手机号
        /// </summary>
        public String Mobile { get; set; }
        /// <summary>
        /// 证件号码
        /// </summary>
        public String IDCardNo { get; set; }
        /// <summary>
        /// 证件类型，身份证101
        /// </summary>
        public String IDCardType { get; set; }
        /// <summary>
        /// 真实姓名
        /// </summary>
        public String RealName { get; set; }
    }
}
