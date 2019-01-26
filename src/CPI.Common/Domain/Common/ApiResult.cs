using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Domain.Common
{
    /// <summary>
    /// 表示Api执行结果
    /// </summary>
    [Serializable]
    public class ApiResult
    {
        /// <summary>
        /// 表示结果状态值
        /// </summary>
        public String Status { get; set; }
        /// <summary>
        /// 表示错误描述信息
        /// </summary>
        public String ErrMsg { get; set; }
        /// <summary>
        /// 表示返回值
        /// </summary>
        public Object Value { get; set; }
    }
}
