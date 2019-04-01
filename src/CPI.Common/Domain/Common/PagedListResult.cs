using System;
using System.Collections.Generic;
using System.Text;
using ATBase.Core.Collections;

namespace CPI.Common.Domain.Common
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class PagedListResult<T>
    {
        /// <summary>
        /// 
        /// </summary>
        public Int32 PageIndex { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Int32 PageSize { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Int32 TotalCount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<T> Items { get; set; }
    }
}
