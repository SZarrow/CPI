using System.Net.Http;
using System.Threading.Tasks;
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
