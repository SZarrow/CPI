using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CPI.Common;
using CPI.IService.SettleServices;
using Lotus.Core;
using Lotus.Logging;
using Quartz;

namespace CPI.ScheduleJobs.Settle
{
    public class Bill99FireWithdrawJob : CPIJob
    {
        private static readonly ILogger _logger = LogManager.GetLogger();

        protected override async Task Execute()
        {
            await Task.CompletedTask;
        }

        private async Task Print(String content)
        {
            await Console.Out.WriteLineAsync($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}]-[发起提现]：{content}");
        }
    }
}
