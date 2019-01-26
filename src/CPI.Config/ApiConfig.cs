using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace CPI.Config
{
    public static class ApiConfig
    {
        private static IConfigurationRoot Configuration { get; set; }

        static ApiConfig()
        {
            String envPath = GlobalConfig.Environment == EnvironmentType.Production.ToString() ? String.Empty : $".{GlobalConfig.Environment}";
            String configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", $"ApiConfig{envPath}.json");
            if (File.Exists(configFilePath))
            {
                var configBuilder = new ConfigurationBuilder();
                configBuilder.AddJsonFile(configFilePath);
                Configuration = configBuilder.Build();
            }
        }

        /// <summary>
        /// 快钱盈帐通请求地址
        /// </summary>
        public static String Bill99YZTRequestUrl
        {
            get
            {
                return Configuration["X-99bill-YZT:RequestUrl"];
            }
        }
        /// <summary>
        /// 快钱协议支付申请绑卡请求地址
        /// </summary>
        public static String Bill99_AgreePay_ApplyBindCard_RequestUrl
        {
            get
            {
                return Configuration["X-99bill-AgreePay:ApplyBindCard-RequestUrl"];
            }
        }
        /// <summary>
        /// 快钱协议支付绑卡请求地址
        /// </summary>
        public static String Bill99_AgreePay_BindCard_RequestUrl
        {
            get
            {
                return Configuration["X-99bill-AgreePay:BindCard-RequestUrl"];
            }
        }
        /// <summary>
        /// 快钱协议支付支付请求地址
        /// </summary>
        public static String Bill99_AgreePay_Pay_RequestUrl
        {
            get
            {
                return Configuration["X-99bill-AgreePay:Pay-RequestUrl"];
            }
        }
        /// <summary>
        /// 快钱协议支付查询请求地址
        /// </summary>
        public static String Bill99_AgreePay_Query_RequestUrl
        {
            get
            {
                return Configuration["X-99bill-AgreePay:Query-RequestUrl"];
            }
        }
        /// <summary>
        /// 快钱代扣支付请求地址
        /// </summary>
        public static String Bill99_EntrustPay_Pay_RequestUrl
        {
            get
            {
                return Configuration["X-99bill-EntrustPay:Pay-RequestUrl"];
            }
        }

        /// <summary>
        /// 获取快钱单笔代付接口请求地址
        /// </summary>
        public static String Bill99FOSinglePayApplyRequestUrl
        {
            get
            {
                return Configuration["X-99bill-FundOut:SinglePayApply-RequestUrl"];
            }
        }
        /// <summary>
        /// 获取快钱单笔代付接口查询地址
        /// </summary>
        public static String Bill99FOSingleQueryRequestUrl
        {
            get
            {
                return Configuration["X-99bill-FundOut:SingleQuery-RequestUrl"];
            }
        }
        /// <summary>
        /// 双乾代付支付请求地址
        /// </summary>
        public static String EPay95_FundOut_Pay_RequestUrl
        {
            get
            {
                return Configuration["X-95epay-FundOut:Pay-RequestUrl"];
            }
        }
        /// <summary>
        /// 双乾代付支付结果通知地址
        /// </summary>
        public static String EPay95_FundOut_Pay_NotifyUrl
        {
            get
            {
                return Configuration["X-95epay-FundOut:Pay-NotifyUrl"];
            }
        }

        /// <summary>
        /// 获取商户系统申请冻结放款余额的请求地址
        /// </summary>
        public static String SystemMerchantAccountBalanceFreezeRequestUrl
        {
            get
            {
                return Configuration["System-Merchant:AccountBalance-Freeze-RequestUrl"];
            }
        }
    }
}
