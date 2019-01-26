using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Lotus.Validation;
using Newtonsoft.Json;

namespace CPI.Common.Domain.SettleDomain.Bill99
{
    /// <summary>
    /// 退货分账请求类
    /// </summary>
    public class AllotAmountRefundRequest : ValidateModel
    {
        /// <summary>
        /// 分配给接入平台的Id
        /// </summary>
        [Required(ErrorMessage = "AppId字段必需")]
        public String AppId { get; set; }

        /// <summary>
        /// 收款人Id
        /// </summary>
        [Required(ErrorMessage = "PayeeId字段必需")]
        public String PayeeId { get; set; }

        /// <summary>
        /// 提现外部交易号
        /// </summary>
        [Required(ErrorMessage = "WithdrawOutTradeNo字段必需")]
        public String WithdrawOutTradeNo { get; set; }

        /// <summary>
        /// 分账总金额
        /// </summary>
        [Required(ErrorMessage = "TotalAmount字段必需")]
        public Decimal TotalAmount { get; set; }

        /// <summary>
        /// 分账数据
        /// </summary>
        [Required(ErrorMessage = "SettlePeriod字段必需")]
        public String SettlePeriod { get; set; }
    }
}
