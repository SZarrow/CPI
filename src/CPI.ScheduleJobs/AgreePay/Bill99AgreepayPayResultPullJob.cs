using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CPI.Common;
using CPI.Data.PostgreSQL;
using CPI.IService.AgreePay;
using Lotus.Core;
using Lotus.Logging;
using Quartz;

namespace CPI.ScheduleJobs.AgreePay
{
    public class Bill99AgreepayPayResultPullJob : CPIJob
    {
        private static readonly ILogger _logger = LogManager.GetLogger();

        protected async override Task Execute(String traceId)
        {
            _logger.ContinueTrace("CPI.ScheduleJobs", traceId);
            var service = XDI.Resolve<IAgreementPaymentService>();
            var result = service.Pull(20);
            if (result.Success)
            {
                await Print($"成功从快钱拉取{result.Value}条支付结果");
            }
            else
            {
                _logger.Error(TraceType.SCHEDULE.ToString(), CallResultStatus.ERROR.ToString(), $"{this.GetType().FullName}.Execute()", "拉取快钱协议支付结果定时任务", "从快钱拉取协议支付结果失败", result.FirstException);
                await Print($"从快钱拉取协议支付结果失败：{result.FirstException.Message}");
            }
        }

        private async Task Print(String content)
        {
            await Console.Out.WriteLineAsync($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}]-[快钱协议支付]：{content}");
        }
    }
}
