using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CPI.Common;
using CPI.IService.FundOut;
using Lotus.Core;
using Lotus.Logging;
using Quartz;

namespace CPI.ScheduleJobs.FundOut
{
    public class Bill99PaymentResultPullJob : CPIJob
    {
        private static readonly ILogger _logger = LogManager.GetLogger();

        protected override async Task Execute()
        {
            await Task.CompletedTask;
        }

        private async Task Print(String content)
        {
            await Console.Out.WriteLineAsync($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}]-[快钱代付]：{content}");
        }
    }
}
