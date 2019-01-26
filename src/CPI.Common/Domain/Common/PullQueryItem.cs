using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Domain.Common
{
    /// <summary>
    /// 
    /// </summary>
    public struct PullQueryItem
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="outTradeNo"></param>
        /// <param name="createTime"></param>
        public PullQueryItem(String outTradeNo, DateTime createTime)
        {
            this.OutTradeNo = outTradeNo;
            this.CreateTime = createTime;
        }

        /// <summary>
        /// 
        /// </summary>
        public String OutTradeNo { get; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime CreateTime { get; }
    }
}
