using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using CPI.Common.Domain.SettleDomain.Bill99.v1_0;
using CPI.ScheduleJobs.Models;
using CPI.Utils;
using Lotus.Logging;
using Lotus.Net;
using Lotus.Security;

namespace CPI.ScheduleJobs.Settle
{
    public class Bill99PullWithdrawResultJob : CPIJob
    {
        private static readonly ILogger _logger = LogManager.GetLogger();

        protected override Task Execute()
        {
            var bizContent = new
            {
                Count = 20
            };

            var sign = CryptoHelper.MakeSign(JsonUtil.SerializeObject(bizContent).Value, CPIScheduleConfig.AppSecretKey, HashAlgorithmName.SHA1);

            var postData = new
            {
                CPIScheduleConfig.AppId,
                Method = "cpi.settle.personal.withdraw.pullresult",
                Version = "1.1",
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                BizContent = JsonUtil.SerializeObject(bizContent).Value,
                SignType = "RSA",
                Sign = sign.Value
            };

            return _client.PostJsonAsync<CPIGatewayCommonResponse<PersonalWithdrawResultPullResponseV1>>(CPIScheduleConfig.RequestUrl, JsonUtil.SerializeObject(postData).Value).ContinueWith(t0 =>
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
                        _logger.Error("CPI.ScheduleJobs.Settle", "ERROR", $"{this.GetType().FullName}.Execute()", "_client.PostJson(...)", resp.ErrorMessage, resp.FirstException);
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
            Console.Out.WriteLineAsync($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}]-[拉取提现结果]：{content}");
        }
    }
}
