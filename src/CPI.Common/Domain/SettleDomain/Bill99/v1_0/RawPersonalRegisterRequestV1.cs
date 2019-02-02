using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace CPI.Common.Domain.SettleDomain.Bill99.v1_0
{
    /// <summary>
    /// 原始个人账户开户请求类
    /// </summary>
    public class RawPersonalRegisterRequestV1
    {
        /// <summary>
        /// 请求id
        /// </summary>
        public String requestId { get; set; }

        /// <summary>
        /// 收款人Id
        /// </summary>
        public String uId { get; set; }

        /// <summary>
        /// 用户标识，0：企业，1：个人
        /// </summary>
        public String userFlag { get; set; }

        /// <summary>
        /// 平台代码
        /// </summary>
        public String platformCode { get; set; }

        /// <summary>
        /// 证件类型，身份证：101
        /// </summary>
        public String idCardType { get; set; }

        /// <summary>
        /// 证件号码
        /// </summary>
        public String idCardNumber { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        public String name { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        public String mobile { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        public String email { get; set; }
    }
}
