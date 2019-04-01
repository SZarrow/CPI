using CPI.Common;
using ATBase.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.ScheduleJobs.Models
{
    /// <summary>
    /// 通用响应类
    /// </summary>
    [Serializable]
    public sealed class CPIGatewayCommonResponse<T>
    {
        private String _msg;

        /// <summary>
        /// 状态码
        /// </summary>
        [JsonConverter(typeof(StatusToStringJsonConverter))]
        public Int32 Status { get; set; }
        /// <summary>
        /// 状态描述
        /// </summary>
        public String Msg
        {
            get
            {
                if (String.IsNullOrWhiteSpace(_msg))
                {
                    _msg = ErrorCodeDescriptor.GetDescription(this.Status);
                }

                return _msg;
            }
            set
            {
                _msg = value;
            }
        }
        /// <summary>
        /// 响应的内容对象
        /// </summary>
        public T Content { get; set; }
        /// <summary>
        /// 签名
        /// </summary>
        public String Sign { get; set; }
    }
}
