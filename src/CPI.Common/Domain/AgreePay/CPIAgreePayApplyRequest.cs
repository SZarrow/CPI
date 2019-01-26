using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Lotus.Validation;
using Newtonsoft.Json;

namespace CPI.Common.Domain.AgreePay
{
    /// <summary>
    /// 申请支付请求参数类
    /// </summary>
    public class CPIAgreePayApplyRequest : ValidateModel
    {
        /// <summary>
        /// 分配给接入平台的Id
        /// </summary>
        [Required(ErrorMessage = "AppId字段必需")]
        public String AppId { get; set; }

        /// <summary>
        /// 付款人Id
        /// </summary>
        [Required(ErrorMessage = "PayerId字段必需")]
        public String PayerId { get; set; }

        /// <summary>
        /// 外部交易号（商户订单号），最大长度64位
        /// </summary>
        [Required(ErrorMessage = "OutTradeNo字段必需")]
        public String OutTradeNo { get; set; }

        /// <summary>
        /// 付款人银行卡号
        /// </summary>
        [Required(ErrorMessage = "BankCardNo字段必需")]
        public String BankCardNo { get; set; }

        /// <summary>
        /// 付款人真实姓名
        /// </summary>
        [Required(ErrorMessage = "RealName字段必需")]
        public String RealName { get; set; }

        /// <summary>
        /// 付款人身份证号
        /// </summary>
        [Required(ErrorMessage = "IDCardNo字段必需")]
        [StringLength(20, ErrorMessage = "身份证号超出范围")]
        public String IDCardNo { get; set; }

        /// <summary>
        /// 付款人银行预留手机号
        /// </summary>
        [Required(ErrorMessage = "Mobile字段必需")]
        public String Mobile { get; set; }

        /// <summary>
        /// 银行编码
        /// </summary>
        [Required(ErrorMessage = "BankCode字段必需")]
        public String BankCode { get; set; }
    }
}
