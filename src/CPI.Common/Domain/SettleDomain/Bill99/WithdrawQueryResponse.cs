using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Domain.SettleDomain.Bill99
{
    /// <summary>
    /// 
    /// </summary>
    public class WithdrawQueryResponse : CommonResponse
    {
        /// <summary>
        /// 外部交易编号
        /// </summary>
        public String OutTradeNo { get; set; }

        /// <summary>
        /// 提现金额
        /// </summary>
        public Decimal Amount { get; set; }

        /// <summary>
        /// 客户自付手续费
        /// </summary>
        public Decimal CustomerFee { get; set; }

        /// <summary>
        /// 商户代付手续费
        /// </summary>
        public Decimal MerchantFee { get; set; }

        /// <summary>
        /// 银行卡主键 Id
        /// </summary>
        public String MemberBankAcctId { get; set; }

        /// <summary>
        /// 银行卡号
        /// </summary>
        public String BankCardNo { get; set; }

        /// <summary>
        /// 交易摘要
        /// </summary>
        public String Memo { get; set; }
    }
}
