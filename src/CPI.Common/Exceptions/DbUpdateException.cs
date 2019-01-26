using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Exceptions
{
    /// <summary>
    /// 表示数据库更新异常
    /// </summary>
    [Serializable]
    public class DbUpdateException : SystemException
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public DbUpdateException(String message) : this(message, null) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public DbUpdateException(String message, Exception innerException) : base(message, innerException) { }
    }
}
