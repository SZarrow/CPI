using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CPI.Common;
using CPI.IService.SettleServices;
using Lotus.Core;
using Lotus.Logging;

namespace CPI.ScheduleJobs.Settle
{
    //public class Bill99PullRegisterAuditResultJob : CPIJob
    //{
    //    private static readonly ILogger _logger = LogManager.GetLogger();

    //    protected async override Task Execute()
    //    {
    //        var service = XDI.Resolve<IPersonalService>();
    //        var result = service.PullRegisterAuditResult(20);
    //        if (result.Success)
    //        {
    //            await Print($"成功{result.Value}个");
    //        }
    //        else
    //        {
    //            _logger.Error(TraceType.SCHEDULE.ToString(), CallResultStatus.ERROR.ToString(), $"{this.GetType().FullName}.Execute()", "快钱分账开户结果定时拉取任务", "拉取审核结果失败", result.FirstException);
    //            await Print($"失败：{result.FirstException.Message}");
    //        }
    //    }

    //    private async Task Print(String content)
    //    {
    //        await Console.Out.WriteLineAsync($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}]-[拉取审核结果]：{content}");
    //    }
    //}
}
