using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using ATBase.Validation;

namespace CPI.Common.Domain.FundOut.EPay95
{
    /// <summary>
    /// 通知结果
    /// </summary>
    public class PayNotifyResult : ValidateModel
    {
        /// <summary>
        /// 收款信息
        /// </summary>
        [Required(ErrorMessage = "LoanJsonList")]
        public String LoanJsonList { get; set; }
        /// <summary>
        /// 还乾宝平台标识
        /// </summary>
        [Required(ErrorMessage = "PlatformMoneymoremore字段必需")]
        public String PlatformMoneymoremore { get; set; }
        /// <summary>
        /// 外部订单编号
        /// </summary>
        [Required(ErrorMessage = "BatchNo字段必需")]
        [StringLength(20, ErrorMessage = "BatchNo字段不能超过20位")]
        public String BatchNo { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public String Remark { get; set; }
        /// <summary>
        /// 返回码
        /// </summary>
        [Required(ErrorMessage = "ResultCode字段必需")]
        public String ResultCode { get; set; }
        /// <summary>
        /// 返回信息
        /// </summary>
        public String Message { get; set; }
        /// <summary>
        /// 签名信息
        /// </summary>
        [Required(ErrorMessage = "SignInfo字段必需")]
        public String SignInfo { get; set; }
    }
}
