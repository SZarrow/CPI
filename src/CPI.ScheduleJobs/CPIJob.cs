using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CPI.Common;
using CPI.Data.PostgreSQL;
using Lotus.Core;
using Lotus.Logging;
using Quartz;

namespace CPI.ScheduleJobs
{
    public abstract class CPIJob : IJob
    {
        protected static readonly HttpClient _client = new HttpClient();

        protected CPIJob() { }

        public Task Execute(IJobExecutionContext context)
        {
            return Execute();
        }

        protected abstract Task Execute();
    }
}
