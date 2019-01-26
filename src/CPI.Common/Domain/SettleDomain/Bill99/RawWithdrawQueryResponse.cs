using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace CPI.Common.Domain.SettleDomain.Bill99
{
    /// <summary>
    /// 账户提现查询响应类
    /// </summary>
    public class RawWithdrawQueryResponse : YZTCommonResponse
    {
        /// <summary>
        /// 外部交易编号
        /// </summary>
        [JsonProperty("outTradeNo")]
        public String OutTradeNo { get; set; }

        /// <summary>
        /// 提现金额
        /// </summary>
        [JsonProperty("amount")]
        [JsonConverter(typeof(AmountToCentJsonConverter))]
        public Decimal Amount { get; set; }

        /// <summary>
        /// 客户自付手续费
        /// </summary>
        [JsonProperty("customerFee")]
        [JsonConverter(typeof(AmountToCentJsonConverter))]
        public Decimal CustomerFee { get; set; }

        /// <summary>
        /// 商户代付手续费
        /// </summary>
        [JsonProperty("merchantFee")]
        [JsonConverter(typeof(AmountToCentJsonConverter))]
        public Decimal MerchantFee { get; set; }

        /// <summary>
        /// 银行卡主键 Id
        /// </summary>
        [JsonProperty("memberBankAcctId")]
        public String MemberBankAcctId { get; set; }

        /// <summary>
        /// 银行卡号
        /// </summary>
        [JsonProperty("bankAcctId")]
        public String BankCardNo { get; set; }

        /// <summary>
        /// 交易摘要
        /// </summary>
        [JsonProperty("memo")]
        public String Memo { get; set; }

        /// <summary>
        /// 交易状态，1：成功，2：失败，3：处理中
        /// </summary>
        [JsonProperty("status")]
        public String Status { get; set; }

        /// <summary>
        /// 交易描述
        /// </summary>
        [JsonProperty("tradeDescription")]
        public String Msg { get; set; }
    }
}
