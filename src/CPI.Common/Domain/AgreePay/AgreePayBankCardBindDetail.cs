using System;
using System.Collections.Generic;
using System.Text;
using CPI.Common.Models;

namespace CPI.Common.Domain.AgreePay
{
    /// <summary>
    /// 协议支付绑卡详细信息
    /// </summary>
    [Serializable]
    public class AgreePayBankCardBindDetail
    {
        /// <summary>
        /// 付款人Id
        /// </summary>
        public String PayerId { get; set; }
        /// <summary>
        /// 外部交易编号
        /// </summary>
        public String OutTradeNo { get; set; }
        /// <summary>
        /// 银行卡号
        /// </summary>
        public String BankCardNo { get; set; }
        /// <summary>
        /// 付款人身份证号
        /// </summary>
        public String IDCardNo { get; set; }
        /// <summary>
        /// 付款人真实姓名
        /// </summary>
        public String RealName { get; set; }
        /// <summary>
        /// 支付通道编码
        /// </summary>
        public String PayChannelCode { get; set; }
        /// <summary>
        /// 手机号
        /// </summary>
        public String Mobile { get; set; }
        /// <summary>
        /// 支付令牌
        /// </summary>
        public String PayToken { get; set; }
        /// <summary>
        /// 绑定状态
        /// </summary>
        public String BindStatus { get; set; }
        /// <summary>
        /// 申请绑定时间
        /// </summary>
        public DateTime ApplyTime { get; set; }
    }
}
