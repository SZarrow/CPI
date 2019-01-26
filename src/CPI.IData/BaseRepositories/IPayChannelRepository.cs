using System;
using System.Collections.Generic;
using System.Text;
using CPI.Common.Models;

namespace CPI.IData.BaseRepositories
{
    /// <summary>
    /// 支付通道表的数据访问接口
    /// </summary>
    public interface IPayChannelRepository : IUnitOfWork<PayChannel>
    {
    }
}
