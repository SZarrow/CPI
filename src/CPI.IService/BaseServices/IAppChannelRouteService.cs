using System;
using System.Collections.Generic;
using System.Text;
using CPI.Common.Models;
using ATBase.Core;

namespace CPI.IService.BaseServices
{
    /// <summary>
    /// 系统应用通道路由服务
    /// </summary>
    public interface IAppChannelRouteService
    {
        /// <summary>
        /// 根据appid获取路由表中配置的通道编码
        /// </summary>
        /// <param name="appid">AppId</param>
        PayChannel GetPayChannel(String appid);
    }
}
