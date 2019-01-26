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

        protected async override Task Execute(String traceId)
        {
            _logger.ContinueTrace("CPI.ScheduleJobs", traceId);
            var service = XDI.Resolve<IAllotAmountWithdrawService>();
            var result = service.FireWithdraw(10);
            if (result.Success)
            {
                await Print($"成功{result.Value}个");
            }
            else
            {
                _logger.Error(TraceType.SCHEDULE.ToString(), CallResultStatus.ERROR.ToString(), $"{this.GetType().FullName}.Execute()", "快钱提现定时任务", "发起提现失败", result.FirstException);
                await Print($"失败：{result.FirstException.Message}");
            }
        }

        private async Task Print(String content)
        {
            await Console.Out.WriteLineAsync($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}]-[发起提现]：{content}");
        }
    }
}
