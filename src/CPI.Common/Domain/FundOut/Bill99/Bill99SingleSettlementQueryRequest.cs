using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using ATBase.Serialization;
using ATBase.Validation;

namespace CPI.Common.Domain.FundOut.Bill99
{
    /// <summary>
    /// 快钱单笔代付查询请求类
    /// </summary>
    [XElement("pay2BankSearchRequestParam")]
    public class Bill99SingleSettlementQueryRequest : ValidateModel
    {
        /// <summary>
        /// 当前页码，范围大于等于1，必填
        /// </summary>
        [XElement("targetPage")]
        [Required(ErrorMessage = "PageIndex字段必需")]
        public Int32 PageIndex { get; set; }

        /// <summary>
        /// 每页显示的记录数，范围[1,20]，必填
        /// </summary>
        [XElement("pageSize")]
        [Required(ErrorMessage = "PageSize字段必需")]
        public Int32 PageSize { get; set; }

        /// <summary>
        /// 支付申请接口的调用起始时间，格式：yyyy-MM-dd HH:mm:ss，必填
        /// </summary>
        [XElement("startDate")]
        [Required(ErrorMessage = "StartTime字段必需")]
        public String StartTime { get; set; }

        /// <summary>
        /// 支付申请接口的调用截至时间，格式：yyyy-MM-dd HH:mm:ss，必填
        /// </summary>
        [XElement("endDate")]
        [Required(ErrorMessage = "EndTime字段必需")]
        public String EndTime { get; set; }

        /// <summary>
        /// 订单编号
        /// </summary>
        [XElement("orderId")]
        public String OrderNo { get; set; }

        /// <summary>
        /// 收款人姓名
        /// </summary>
        [XElement("creditName")]
        public String CreditName { get; set; }

        /// <summary>
        /// 银行名称
        /// </summary>
        [XElement("bankName")]
        public String BankName { get; set; }

        /// <summary>
        /// 银行卡号
        /// </summary>
        [XElement("bankAcctId")]
        public String BankCardNo { get; set; }
    }
}
