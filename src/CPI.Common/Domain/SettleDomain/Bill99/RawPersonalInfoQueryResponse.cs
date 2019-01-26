using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace CPI.Common.Domain.SettleDomain.Bill99
{
    /// <summary>
    /// 
    /// </summary>
    public class RawPersonalInfoQueryResponse : YZTCommonResponse
    {
        /// <summary>
        /// 证件类型
        /// </summary>
        [JsonProperty("idCardType")]
        public String IDCardType { get; set; }

        /// <summary>
        /// 证件号码
        /// </summary>
        [JsonProperty("idCardNumber")]
        public String IDCardNo { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        [JsonProperty("name")]
        public String RealName { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        [JsonProperty("mobile")]
        public String Mobile { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        [JsonProperty("email")]
        public String Email { get; set; }

        /// <summary>
        /// 审核状态，01：待审核，02：待复审，03：审核通过
        /// </summary>
        [JsonProperty("auditStatus")]
        public String AuditStatus { get; set; }
    }
}
