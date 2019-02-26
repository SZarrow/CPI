using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Configuration;

namespace CPI.Config
{
    /// <summary>
    /// 全局配置类
    /// </summary>
    public static class GlobalConfig
    {
        private static String _envType = EnvironmentType.Development.ToString();
        private static readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        static GlobalConfig()
        {
            Environment = EnvironmentType.Development.ToString();
        }

        /// <summary>
        /// 获取或设置环境，默认为Development
        /// </summary>
        public static String Environment
        {
            get
            {
                return _envType;
            }
            set
            {
                _envType = value;

                String configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", $"GlobalConfig{GetEnvPath(_envType)}.json");
                if (File.Exists(configFilePath))
                {
                    try
                    {
                        var cb = new ConfigurationBuilder();
                        var config = cb.AddJsonFile(configFilePath).Build();
                        _lock.EnterWriteLock();
                        Configuration = config;
                    }
                    catch { }
                    finally
                    {
                        if (_lock.IsWriteLockHeld)
                        {
                            _lock.ExitWriteLock();
                        }
                    }
                }
            }
        }

        private static String GetEnvPath(String environment)
        {
            return environment == EnvironmentType.Production.ToString() ? String.Empty : $".{environment}";
        }

        private static IConfigurationRoot Configuration { get; set; }

        /// <summary>
        /// 获取通用的远程证书回调函数
        /// </summary>
        public static readonly Func<Object, X509Certificate, X509Chain, SslPolicyErrors, Boolean> CommonRemoteCertificateValidationCallback = (t1, t2, t3, t4) => true;
        /// <summary>
        /// 默认支付通道编码
        /// </summary>
        public static String DefaultPayChannelCode
        {
            get
            {
                return Configuration["DefaultPayChannelCode"];
            }
        }
        /// <summary>
        /// 支付通道费阈值，默认2.2
        /// </summary>
        public static Decimal PayChannelFeeThreshold
        {
            get
            {
                if (Decimal.TryParse(Configuration["PayChannelFeeThreshold"], out Decimal result))
                {
                    return result;
                }

                return 2.2m;
            }
        }
        /// <summary>
        /// 获取快钱进件1.0平台代码
        /// </summary>
        public static String X99bill_COE_v1_PlatformCode
        {
            get
            {
                return Configuration["X-99bill-COE-v1.0:Hehua-PlatformCode"];
            }
        }
        /// <summary>
        /// 快钱HAT平台商户号
        /// </summary>
        public static String X99bill_HAT_PlatformCode
        {
            get
            {
                return Configuration["X-99bill-HAT:Hehua-PlatformCode"];
            }
        }
        /// <summary>
        /// 获取快钱分账荷花平台编码
        /// </summary>
        public static String X99bill_YZT_PlatformCode
        {
            get
            {
                return Configuration["X-99bill-YZT:Hehua-PlatformCode"];
            }
        }
        /// <summary>
        /// 获取快钱支付通道编码
        /// </summary>
        public static String X99bill_PayChannelCode
        {
            get
            {
                return "99bill";
            }
        }
        /// <summary>
        /// 获取快钱代付荷花会员编号
        /// </summary>
        public static String X99bill_FundOut_Hehua_MemberCode
        {
            get
            {
                return Configuration["X-99bill-FundOut:Hehua-MemberCode"];
            }
        }
        /// <summary>
        /// 双乾代付还乾宝平台标识
        /// </summary>
        public static String X95epay_FundOut_Hehua_PlatformMoneymoremore
        {
            get
            {
                return Configuration["X-95epay-FundOut:Hehua-PlatformMoneymoremore"];
            }
        }
        /// <summary>
        /// 快钱盈帐通提现最小金额
        /// </summary>
        public static Decimal X99bill_YZT_WithdrawMinAmount
        {
            get
            {
                String withdrawMinAmountValue = Configuration["X-99bill-YZT:WithdrawMinAmount"];
                if (Decimal.TryParse(withdrawMinAmountValue, out Decimal result))
                {
                    return result;
                }

                return 1m;
            }
        }
        /// <summary>
        /// 快钱协议支付最小金额
        /// </summary>
        public static Decimal X99bill_AgreePay_PayMinAmount
        {
            get
            {
                String payMinAmountValue = Configuration["X-99bill-AgreePay:PayMinAmount"];
                if (Decimal.TryParse(payMinAmountValue, out Decimal result))
                {
                    return result;
                }

                return 1m;
            }
        }
        /// <summary>
        /// 快钱协议支付荷花商户Id
        /// </summary>
        public static String X99bill_AgreePay_Hehua_MerchantId
        {
            get
            {
                return Configuration["X-99bill-AgreePay:Hehua-MerchantId"];
            }
        }
        /// <summary>
        /// 快钱协议支付荷花终端Id
        /// </summary>
        public static String X99bill_AgreePay_Hehua_TerminalId
        {
            get
            {
                return Configuration["X-99bill-AgreePay:Hehua-TerminalId"];
            }
        }
        /// <summary>
        /// 快钱代扣荷花商户Id
        /// </summary>
        public static String X99bill_EntrustPay_Hehua_MerchantId
        {
            get
            {
                return Configuration["X-99bill-EntrustPay:Hehua-MerchantId"];
            }
        }
        /// <summary>
        /// 快钱代扣荷花终端Id
        /// </summary>
        public static String X99bill_EntrustPay_Hehua_TerminalId
        {
            get
            {
                return Configuration["X-99bill-EntrustPay:Hehua-TerminalId"];
            }
        }
        /// <summary>
        /// 快钱代扣最小金额
        /// </summary>
        public static Decimal X99bill_EntrustPay_PayMinAmount
        {
            get
            {
                String payMinAmountValue = Configuration["X-99bill-EntrustPay:PayMinAmount"];
                if (Decimal.TryParse(payMinAmountValue, out Decimal result))
                {
                    return result;
                }

                return 1m;
            }
        }
        /// <summary>
        /// 易宝支付代付商户号
        /// </summary>
        public static String YeePay_FundOut_MerchantNo
        {
            get
            {
                return Configuration["YeePay-FundOut:Hehua-MerchantNo"];
            }
        }
        /// <summary>
        /// 易宝支付代付AppKey
        /// </summary>
        public static String YeePay_FundOut_AppKey
        {
            get
            {
                return Configuration["YeePay-FundOut:Hehua-AppKey"];
            }
        }
    }
}
