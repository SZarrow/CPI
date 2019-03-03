using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace CPI.Config
{
    /// <summary>
    /// 密钥配置类
    /// </summary>
    public static class KeyConfig
    {
        private static readonly IConfiguration Configuration;

        static KeyConfig()
        {
            String envPath = GlobalConfig.Environment == EnvironmentType.Production.ToString() ? String.Empty : $".{GlobalConfig.Environment}";
            String configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", $"KeyConfig{envPath}.json");
            if (File.Exists(configFilePath))
            {
                var configBuilder = new ConfigurationBuilder();
                configBuilder.AddJsonFile(configFilePath);
                Configuration = configBuilder.Build();
            }
        }

        /// <summary>
        /// 快钱盈帐通公钥
        /// </summary>
        public static String Bill99YZTPublicKey
        {
            get
            {
                return Configuration["X-99bill-YZT:YZT-PublicKey"];
            }
        }

        /// <summary>
        /// 快钱盈帐通荷花私钥
        /// </summary>
        public static String Bill99YZTHehuaPrivateKey
        {
            get
            {
                return Configuration["X-99bill-YZT:Hehua-PrivateKey"];
            }
        }

        /// <summary>
        /// 快钱盈帐通荷花公钥
        /// </summary>
        public static String Bill99YZTHehuaPublicKey
        {
            get
            {
                return Configuration["X-99bill-YZT:Hehua-PublicKey"];
            }
        }

        /// <summary>
        /// 快钱进件1.0快钱公钥
        /// </summary>
        public static String Bill99_COE_v1_PublicKey
        {
            get
            {
                return Configuration["X-99bill-COE-v1.0:COE-PublicKey"];
            }
        }

        /// <summary>
        /// 快钱进件1.0荷花公钥
        /// </summary>
        public static String Bill99_COE_v1_Hehua_PublicKey
        {
            get
            {
                return Configuration["X-99bill-COE-v1.0:Hehua-PublicKey"];
            }
        }

        /// <summary>
        /// 快钱进件1.0荷花私钥
        /// </summary>
        public static String Bill99_COE_v1_Hehua_PrivateKey
        {
            get
            {
                return Configuration["X-99bill-COE-v1.0:Hehua-PrivateKey"];
            }
        }

        /// <summary>
        /// 快钱HAT平台公钥
        /// </summary>
        public static String Bill99_HAT_PublicKey
        {
            get
            {
                return Configuration["X-99bill-HAT:HAT-PublicKey"];
            }
        }

        /// <summary>
        /// 快钱HAT平台荷花公钥
        /// </summary>
        public static String Bill99_HAT_Hehua_PublicKey
        {
            get
            {
                return Configuration["X-99bill-HAT:Hehua-PublicKey"];
            }
        }

        /// <summary>
        /// 快钱HAT平台荷花私钥
        /// </summary>
        public static String Bill99_HAT_Hehua_PrivateKey
        {
            get
            {
                return Configuration["X-99bill-HAT:Hehua-PrivateKey"];
            }
        }

        /// <summary>
        /// 快钱协议支付私钥文件路径
        /// </summary>
        public static String Bill99_AgreePay_PrivateKeyFilePath
        {
            get
            {
                return Configuration["X-99bill-AgreePay:PrivateKeyFilePath"];
            }
        }

        /// <summary>
        /// 快钱协议支付私钥文件密码
        /// </summary>
        public static String Bill99_AgreePay_PrivateKeyFilePassword
        {
            get
            {
                return Configuration["X-99bill-AgreePay:PrivateKeyFilePassword"];
            }
        }

        /// <summary>
        /// 快钱代扣私钥文件路径
        /// </summary>
        public static String Bill99_EntrustPay_PrivateKeyFilePath
        {
            get
            {
                return Configuration["X-99bill-EntrustPay:PrivateKeyFilePath"];
            }
        }

        /// <summary>
        /// 快钱代扣私钥文件密码
        /// </summary>
        public static String Bill99_EntrustPay_PrivateKeyFilePassword
        {
            get
            {
                return Configuration["X-99bill-EntrustPay:PrivateKeyFilePassword"];
            }
        }

        /// <summary>
        /// 快钱代付公钥
        /// </summary>
        public static String Bill99FOPublicKey
        {
            get
            {
                return Configuration["X-99bill-FundOut:X-99bill-PublicKey"];
            }
        }

        /// <summary>
        /// 获取荷花代付私钥
        /// </summary>
        public static String Bill99FOHehuaPrivateKey
        {
            get
            {
                return Configuration["X-99bill-FundOut:Hehua-PrivateKey"];
            }
        }

        /// <summary>
        /// 双乾代付公钥
        /// </summary>
        public static String EPay95_FundOut_PublicKey
        {
            get
            {
                return Configuration["X-95epay-FundOut:X-95epay-PublicKey"];
            }
        }

        /// <summary>
        /// 荷花在双乾的代付私钥
        /// </summary>
        public static String EPay95_FundOut_Hehua_PrivateKey
        {
            get
            {
                return Configuration["X-95epay-FundOut:Hehua-PrivateKey"];
            }
        }


        /// <summary>
        /// 获取荷花代付公钥
        /// </summary>
        public static String Bill99FOHehuaPublicKey
        {
            get
            {
                return Configuration["X-99bill-FundOut:Hehua-PublicKey"];
            }
        }

        /// <summary>
        /// CPI通用公钥
        /// </summary>
        public static String CPICommonPublicKey
        {
            get
            {
                return Configuration["CPI-Common:PublicKey"];
            }
        }

        /// <summary>
        /// CPI通用私钥
        /// </summary>
        public static String CPICommonPrivateKey
        {
            get
            {
                return Configuration["CPI-Common:PrivateKey"];
            }
        }

        /// <summary>
        /// 荷花商户系统签名使用的Key
        /// </summary>
        public static String HehuaMerchantSignKey
        {
            get
            {
                return Configuration["System-Merchant:Sign-Key"];
            }
        }

        /// <summary>
        /// 易宝支付代付公钥
        /// </summary>
        public static String YeePay_FundOut_PublicKey
        {
            get
            {
                return Configuration["YeePay-FundOut:YeePay-PublicKey"];
            }
        }

        /// <summary>
        /// 易宝支付代付荷花私钥
        /// </summary>
        public static String YeePay_FundOut_Hehua_PrivateKey
        {
            get
            {
                return Configuration["YeePay-FundOut:Hehua-PrivateKey"];
            }
        }

        /// <summary>
        /// 易宝协议支付公钥
        /// </summary>
        public static String YeePay_AgreePay_PublicKey
        {
            get
            {
                return Configuration["YeePay-AgreePay:YeePay-PublicKey"];
            }
        }

        /// <summary>
        /// 易宝协议支付荷花私钥
        /// </summary>
        public static String YeePay_AgreePay_Hehua_PrivateKey
        {
            get
            {
                return Configuration["YeePay-AgreePay:Hehua-PrivateKey"];
            }
        }
    }
}
