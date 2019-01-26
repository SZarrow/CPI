using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Exceptions
{
    /// <summary>
    /// 请求异常
    /// </summary>
    [Serializable]
    public class RequestException : SystemException
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public RequestException(String message) : this(message, null) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public RequestException(String message, Exception innerException) : base(message, innerException) { }
    }
}
