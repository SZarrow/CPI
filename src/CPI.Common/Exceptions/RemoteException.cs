using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Exceptions
{
    /// <summary>
    /// 
    /// </summary>
    public class RemoteException : SystemException
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public RemoteException(String message) : this(message, null) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public RemoteException(String message, Exception innerException) : base(message, innerException) { }
    }
}
