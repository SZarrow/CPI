using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace CPI.Common.Domain.SettleDomain.Bill99
{
    /// <summary>
    /// 原始个人账户注册响应类
    /// </summary>
    public class RawPersonalRegisterResponse : YZTCommonResponse
    {
        /// <summary>
        /// 蝶巢侧用户 id
        /// </summary>
        [JsonProperty("openId")]
        public String OpenId { get; set; }

        /// <summary>
        /// 审核状态
        /// </summary>
        [JsonProperty("auditStatus")]
        public String AuditStatus { get; set; }
    }
}
