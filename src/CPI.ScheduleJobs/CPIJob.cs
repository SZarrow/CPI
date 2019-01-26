using System;
using System.Collections.Generic;
using System.Linq;
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
        private static ILogger _logger = LogManager.GetLogger();

        protected CPIJob() { }

        public async Task Execute(IJobExecutionContext context)
        {
            var ins = new Object();
            XDI.CreateScope(ins);
            XDI.Scope(typeof(CPIDbContext));
            String traceId = Guid.NewGuid().ToString("N");
            _logger.StartTrace("CPI.ScheduleJobs", traceId, "127.0.0.1");
            await Execute(traceId);
            _logger.StopTrace();
            XDI.Release(ins);
        }

        protected abstract Task Execute(String traceId);
    }
}
