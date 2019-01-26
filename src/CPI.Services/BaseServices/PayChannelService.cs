using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPI.Common.Models;
using CPI.IData.BaseRepositories;
using CPI.IService.BaseServices;

namespace CPI.Services.BaseServices
{
    public class PayChannelService : IPayChannelService
    {
        private IPayChannelRepository _payChannelRepository = null;

        public IEnumerable<PayChannel> GetAllChannels()
        {
            return _payChannelRepository.QueryProvider.ToList();
        }
    }
}
