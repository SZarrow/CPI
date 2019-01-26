using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Exceptions
{
    /// <summary>
    /// 表示数据库查询异常
    /// </summary>
    [Serializable]
    public class DbQueryException : SystemException
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public DbQueryException(String message) : this(message, null) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public DbQueryException(String message, Exception innerException) : base(message, innerException) { }
    }
}
