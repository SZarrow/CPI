using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CPI.Data.PostgreSQL;
using Lotus.Core;
using Lotus.Logging;
using Quartz;

namespace CPI.ScheduleJobs
{
    public abstract class CPIJob : IJob
    {
        private static readonly ILogger _logger = LogManager.GetLogger();
        protected static readonly HttpClient _client = new HttpClient();

        protected CPIJob() { }

        public Task Execute(IJobExecutionContext context)
        {
            //try
            //{
            //    await Execute();
            //}
            //catch (Exception ex)
            //{
            //    _logger.Error("CPI.ScheduleJobs", "ERROR", $"{this.GetType().FullName}.Execute()", "Execute()", "定时调度任务执行出现异常", ex);
            //}

            return Execute();
        }

        protected abstract Task Execute();
    }
}
