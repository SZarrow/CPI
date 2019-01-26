using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common
{
    /// <summary>
    /// 通用响应类基类
    /// </summary>
    public abstract class CommonResponse
    {
        /// <summary>
        /// 状态
        /// </summary>
        public String Status { get; set; }
        /// <summary>
        /// 状态描述
        /// </summary>
        public String Msg { get; set; }
    }
}
