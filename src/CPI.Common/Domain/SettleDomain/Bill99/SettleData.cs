using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Lotus.Validation;
using Newtonsoft.Json;

namespace CPI.Common.Domain.SettleDomain.Bill99
{
    /// <summary>
    /// 分账数据实体类
    /// </summary>
    public class SettleData : ValidateModel
    {
        /// <summary>
        /// 收款账户Id
        /// </summary>
        [Required(ErrorMessage = "MerchantUid字段必需")]
        public String MerchantUid { get; set; }

        /// <summary>
        /// 外部子单编号
        /// </summary>
        [Required(ErrorMessage = "OutSubOrderNo字段必需")]
        public String OutSubOrderNo { get; set; }

        /// <summary>
        /// 分账金额
        /// </summary>
        [Required(ErrorMessage = "Amount字段必需")]
        public Decimal Amount { get; set; }

        /// <summary>
        /// 结算周期
        /// </summary>
        [Required(ErrorMessage = "SettlePeriod字段必需")]
        public String SettlePeriod { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override Boolean Validate()
        {
            if (this.Amount <= 0)
            {
                return false;
            }

            return base.Validate();
        }
    }
}
