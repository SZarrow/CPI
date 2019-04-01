using System;
using System.Configuration;
using System.IO;
using CPI.Config;
using ATBase.Core;
using ATBase.Schedule;

namespace CPI.ScheduleJobs
{
    class Program
    {
        static void Main(String[] args)
        {
            XDI.Run();

            GlobalConfig.Environment = ConfigurationManager.AppSettings["Environment"];

            String jobsConfigFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CPI.ScheduleJobs.Development.config");
            if (GlobalConfig.Environment == EnvironmentType.Production.ToString())
            {
                jobsConfigFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CPI.ScheduleJobs.config");
            }

            var service = new QuartzService(jobsConfigFilePath);
            service.Start(null);
            Console.WriteLine("CPI定时调度服务已启动...");
            Console.ReadKey();
        }
    }
}
