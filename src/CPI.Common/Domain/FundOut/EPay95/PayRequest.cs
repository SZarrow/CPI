using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using ATBase.Validation;
using Newtonsoft.Json;

namespace CPI.Common.Domain.FundOut.EPay95
{
    /// <summary>
    /// 代付支付请求类
    /// </summary>
    public class PayRequest : ValidateModel
    {
        /// <summary>
        /// 分配给接入平台的Id
        /// </summary>
        [Required(ErrorMessage = "AppId字段必需")]
        public String AppId { get; set; }

        /// <summary>
        /// 金额
        /// </summary>
        [Required(ErrorMessage = "Amount字段必需")]
        public String Amount { get; set; }
        /// <summary>
        /// 平台订单号
        /// </summary>
        [Required(ErrorMessage = "OutTradeNo字段必需")]
        public String OutTradeNo { get; set; }
        /// <summary>
        /// 商户编号
        /// </summary>
        [Required(ErrorMessage = "MerchantNo字段必需")]
        public String MerchantNo { get; set; }
        /// <summary>
        /// 手机号
        /// </summary>
        [Required(ErrorMessage = "Mobile字段必需")]
        public String Mobile { get; set; }
        /// <summary>
        /// 收款人真实姓名
        /// </summary>
        [Required(ErrorMessage = "RealName字段必需")]
        public String RealName { get; set; }
        /// <summary>
        /// 收款人身份证号
        /// </summary>
        [Required(ErrorMessage = "IDCardNo字段必需")]
        public String IDCardNo { get; set; }
        /// <summary>
        /// 收款人银行卡号
        /// </summary>
        [Required(ErrorMessage = "BankCardNo字段必需")]
        public String BankCardNo { get; set; }
        /// <summary>
        /// 备注，可选
        /// </summary>
        public String Remark { get; set; }
    }
}
