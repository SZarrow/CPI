using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace CPI.Common.Domain.SettleDomain.Bill99
{
    /// <summary>
    /// 个人账户信息查询响应类
    /// </summary>
    public class PersonalInfoQueryResponse : CommonResponse
    {
        /// <summary>
        /// 收款人Id
        /// </summary>
        public String PayeeId { get; set; }

        /// <summary>
        /// 证件类型
        /// </summary>
        public String IDCardType { get; set; }

        /// <summary>
        /// 证件号码
        /// </summary>
        public String IDCardNo { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        public String RealName { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        public String Mobile { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        public String Email { get; set; }
    }
}
