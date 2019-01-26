using System;
using System.Collections.Generic;
using System.Text;
using CPI.Common.Models;

namespace CPI.IService.BaseServices
{
    /// <summary>
    /// 支付通道服务接口
    /// </summary>
    public interface IPayChannelService
    {
        /// <summary>
        /// 获取所有通道数据
        /// </summary>
        IEnumerable<PayChannel> GetAllChannels();
    }
}
