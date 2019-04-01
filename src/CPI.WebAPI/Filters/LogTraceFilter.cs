using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CPI.Common;
using ATBase.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CPI.WebAPI.Filters
{
    public sealed class LogTraceFilter : IActionFilter
    {
        private static readonly ILogger _logger = LogManager.GetLogger();

        public void OnActionExecuting(ActionExecutingContext context)
        {
            String ipAndPort = $"{context.HttpContext.Connection.RemoteIpAddress.ToIPString()}:{context.HttpContext.Connection.RemotePort}";
            _logger.StartTrace("CPI.WebAPI", Activity.Current?.Id ?? context.HttpContext.TraceIdentifier, ipAndPort);
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            _logger.StopTrace();
            context.HttpContext.Response.Headers.Remove("Server");
            context.HttpContext.Response.Headers["Server"] = "Unix";
        }
    }
}
