using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace CPI.Common.Domain.FundOut.EPay95
{
    /// <summary>
    /// 代付结果
    /// </summary>
    public class EPay95PayReturnResult
    {
        /// <summary>
        /// 收款信息
        /// </summary>
        [JsonConverter(typeof(FundOutLoanInfoJsonConverter))]
        public FundOutLoanInfo LoanJsonList { get; set; }
        /// <summary>
        /// 还乾宝平台标识
        /// </summary>
        public String PlatformMoneymoremore { get; set; }
        /// <summary>
        /// 标号（外部订单编号）
        /// </summary>
        public String BatchNo { get; set; }
        /// <summary>
        /// 自定义备注，选填
        /// </summary>
        public String Remark { get; set; }
        /// <summary>
        /// 返回码
        /// </summary>
        public String ResultCode { get; set; }
        /// <summary>
        /// 返回信息
        /// </summary>
        public String Message { get; set; }
        /// <summary>
        /// 签名信息
        /// </summary>
        public String SignInfo { get; set; }
    }
}
