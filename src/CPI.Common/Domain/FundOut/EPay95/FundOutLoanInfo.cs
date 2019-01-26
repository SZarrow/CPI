using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Common.Domain.FundOut.EPay95
{
    /// <summary>
    /// 代付收款信息
    /// </summary>
    public class FundOutLoanInfo
    {
        /// <summary>
        /// 金额
        /// </summary>
        public String Amount { get; set; }
        /// <summary>
        /// 平台订单号
        /// </summary>
        public String OrderNo { get; set; }
        /// <summary>
        /// 手机号
        /// </summary>
        public String Mobile { get; set; }
        /// <summary>
        /// 收款人真实姓名
        /// </summary>
        public String RealName { get; set; }
        /// <summary>
        /// 收款人证件号
        /// </summary>
        public String IdentificationNo { get; set; }
        /// <summary>
        /// 收款人银行卡号
        /// </summary>
        public String CardNumber { get; set; }
        /// <summary>
        /// 类型，0：个人，1：企业
        /// </summary>
        public String Type { get; set; } = "0";
    }
}
