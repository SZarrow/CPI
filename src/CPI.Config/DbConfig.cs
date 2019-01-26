using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace CPI.Config
{
    /// <summary>
    /// 数据库配置类
    /// </summary>
    public static class DbConfig
    {
        private static readonly IConfiguration _config;

        static DbConfig()
        {
            String envPath = GlobalConfig.Environment == EnvironmentType.Production.ToString() ? String.Empty : $".{GlobalConfig.Environment}";
            String configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", $"DbConfig{envPath}.json");
            if (File.Exists(configFilePath))
            {
                var configBuilder = new ConfigurationBuilder();
                configBuilder.AddJsonFile(configFilePath);
                _config = configBuilder.Build();
            }
        }

        /// <summary>
        /// 获取PostgreSQL数据库连接字符串
        /// </summary>
        public static String PgSQLDbConnectionString
        {
            get
            {
                return _config["PgSQLDbConnectionString"];
            }
        }
    }
}
