using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CPI.ScheduleJobs.Models;
using CPI.Utils;
using Lotus.Security;
using Lotus.Net;
using Lotus.Logging;

namespace CPI.ScheduleJobs.AgreePay
{
    public class YeePayAgreepayRefundResultPullJob : CPIJob
    {
        private static readonly ILogger _logger = LogManager.GetLogger();

        protected override Task Execute()
        {
            var bizContent = new
            {
                Count = 100
            };

            var sign = CryptoHelper.MakeSign(JsonUtil.SerializeObject(bizContent).Value, CPIScheduleConfig.AppSecretKey, HashAlgorithmName.SHA1);

            var postData = new
            {
                CPIScheduleConfig.AppId,
                Method = "cpi.agreepay.refundresult.pull.yeepay",
                Version = "1.0",
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                BizContent = JsonUtil.SerializeObject(bizContent).Value,
                SignType = "RSA",
                Sign = sign.Value
            };

            return _client.PostJsonAsync<CPIGatewayCommonResponse<CommonPullResult>>(CPIScheduleConfig.RequestUrl, JsonUtil.SerializeObject(postData).Value).ContinueWith(t0 =>
            {
                if (t0.IsCompleted)
                {
                    if (t0.IsCanceled || t0.IsFaulted)
                    {
                        Print("任务取消或失败");
                        return;
                    }

                    var resp = t0.Result;
                    if (!resp.Success)
                    {
                        _logger.Error("CPI.ScheduleJobs.AgreePay", "ERROR", $"{this.GetType().FullName}.Execute()", "_client.PostJson(...)", resp.ErrorMessage, resp.FirstException);
                        Print(resp.ErrorMessage);
                    }
                    else
                    {
                        Print($"成功拉取 {resp.Value.Content.SuccessCount} 个结果");
                    }
                }
            });
        }

        private void Print(String content)
        {
            Console.Out.WriteLineAsync($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}]-[易宝退款任务]：{content}");
        }
    }
}
