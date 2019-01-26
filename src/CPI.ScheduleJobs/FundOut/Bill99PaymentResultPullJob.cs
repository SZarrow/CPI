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

        protected async override Task Execute(String traceId)
        {
            _logger.ContinueTrace("CPI.ScheduleJobs", traceId);
            var service = XDI.Resolve<IBill99SinglePaymentService>();
            var result = service.Pull(20);
            if (result.Success)
            {
                await Print($"成功从快钱拉取{result.Value}条支付结果");
            }
            else
            {
                _logger.Error(TraceType.SCHEDULE.ToString(), CallResultStatus.ERROR.ToString(), $"{this.GetType().FullName}.Execute()", "快钱代付结果拉取定时任务", "从快钱拉取代付结果失败", result.FirstException);
                await Print($"从快钱拉取代付支付结果失败：{result.FirstException.Message}");
            }
        }

        private async Task Print(String content)
        {
            await Console.Out.WriteLineAsync($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}]-[快钱代付]：{content}");
        }
    }
}
