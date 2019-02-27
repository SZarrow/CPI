using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using CPI.Common;
using CPI.Common.Domain.SettleDomain;
using CPI.Config;
using CPI.Data.PostgreSQL;
using CPI.WebAPI.Filters;
using Lotus.Core;
using Lotus.Web.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CPI.WebAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(options =>
            {
                options.MaxModelValidationErrors = 0;

                options.Filters.Add(typeof(LogTraceFilter));

            }).AddJsonOptions(options =>
            {
                //忽略循环引用
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                //不使用驼峰样式的key
                options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                //设置时间格式
                options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
            }).AddXDI().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(GlobalConfig.CommonRemoteCertificateValidationCallback);
            InitAgreePaymentHttpClient(services);
            InitEntrustPaymentHttpClient(services);
            InitCommonHttpClient(services);

            XDI.AddServices(services);
            XDI.Scope(typeof(CPIDbContext));
        }

        private void InitAgreePaymentHttpClient(IServiceCollection services)
        {
            services.AddHttpClient("AgreePaymentApiHttpClient", client =>
            {
                String auth = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{GlobalConfig.X99bill_AgreePay_Hehua_MerchantId}:{KeyConfig.Bill99_AgreePay_PrivateKeyFilePassword}"));
                client.DefaultRequestHeaders.Add("Authorization", $"Basic {auth}");
                client.DefaultRequestHeaders.Connection.Add("keep-alive");
            }).ConfigurePrimaryHttpMessageHandler(() =>
            {
                String pfxFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, KeyConfig.Bill99_AgreePay_PrivateKeyFilePath);
                if (!File.Exists(pfxFilePath))
                {
                    throw new FileNotFoundException(pfxFilePath);
                }
                var handler = new HttpClientHandler();
                handler.ClientCertificates.Add(new X509Certificate2(pfxFilePath, KeyConfig.Bill99_AgreePay_PrivateKeyFilePassword));
                return handler;
            });
        }

        private void InitEntrustPaymentHttpClient(IServiceCollection services)
        {
            services.AddHttpClient("EntrustPaymentApiHttpClient", client =>
            {
                String auth = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{GlobalConfig.X99bill_EntrustPay_Hehua_MerchantId}:{KeyConfig.Bill99_EntrustPay_PrivateKeyFilePassword}"));
                client.DefaultRequestHeaders.Add("Authorization", $"Basic {auth}");
                client.DefaultRequestHeaders.Connection.Add("keep-alive");
            }).ConfigurePrimaryHttpMessageHandler(() =>
            {
                String pfxFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, KeyConfig.Bill99_EntrustPay_PrivateKeyFilePath);
                if (!File.Exists(pfxFilePath))
                {
                    throw new FileNotFoundException(pfxFilePath);
                }
                var handler = new HttpClientHandler();
                handler.ClientCertificates.Add(new X509Certificate2(pfxFilePath, KeyConfig.Bill99_EntrustPay_PrivateKeyFilePassword));
                return handler;
            });
        }

        private void InitCommonHttpClient(IServiceCollection services)
        {
            services.AddHttpClient("CommonHttpClient");
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDeveloperExceptionPage();
            app.UseStaticFiles();
            app.UseMvc();

            GlobalConfig.Environment = Configuration["Environment"];
            ErrorCodeDescriptor.AddErrorCodeTypes(typeof(ErrorCode));
            XDI.Run();
        }
    }
}
