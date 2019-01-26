using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Exceptions
{
    /// <summary>
    /// 签名异常类
    /// </summary>
    public class SignException : SystemException
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public SignException(String message) : this(message, null) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerEx"></param>
        public SignException(String message, Exception innerEx) : base(message, innerEx) { }
    }
}
