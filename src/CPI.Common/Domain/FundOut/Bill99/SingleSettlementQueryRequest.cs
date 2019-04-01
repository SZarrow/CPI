using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using ATBase.Validation;

namespace CPI.Common.Domain.FundOut.Bill99
{
    /// <summary>
    /// 单笔代付查询请求类
    /// </summary>
    public class SingleSettlementQueryRequest : ValidateModel
    {
        /// <summary>
        /// 分配给接入平台的Id
        /// </summary>
        [Required(ErrorMessage = "AppId字段必需")]
        public String AppId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Required(ErrorMessage = "PageIndex字段必需")]
        [Range(1, Int32.MaxValue, ErrorMessage = "PageIndex超出范围[1,Int32.MaxValue]")]
        public Int32 PageIndex { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Required(ErrorMessage = "PageSize字段必需")]
        [Range(1, 20, ErrorMessage = "PageSize超出范围[1,20]")]
        public Int32 PageSize { get; set; }

        /// <summary>
        /// 订单号
        /// </summary>
        public String OrderNo { get; set; }

        /// <summary>
        /// 查询订单的起始时间
        /// </summary>
        public DateTime? From { get; set; }

        /// <summary>
        /// 查询订单的结束时间
        /// </summary>
        public DateTime? To { get; set; }
    }
}
