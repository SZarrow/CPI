using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using ATBase.Validation;

namespace CPI.Common.Domain.EntrustPay
{
    /// <summary>
    /// 委托代收查询请求类
    /// </summary>
    public class CPIEntrustPayQueryRequest : ValidateModel
    {
        /// <summary>
        /// 分配给接入平台的Id
        /// </summary>
        [Required(ErrorMessage = "AppId字段必需")]
        public String AppId { get; set; }
    }
}
