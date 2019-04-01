using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPI.Common;
using CPI.Common.Models;
using CPI.IData.BaseRepositories;
using CPI.IService.BaseServices;
using ATBase.Core;

namespace CPI.Services.BaseServices
{
    public class AppChannelRouteService : IAppChannelRouteService
    {
        private readonly IAppChannelRouteRepository _appChannelRouteRepository = null;
        private readonly IPayChannelRepository _payChannelRepository = null;

        public PayChannel GetPayChannel(String appid)
        {
            if (String.IsNullOrWhiteSpace(appid))
            {
                return null;
            }

            return (from t0 in _appChannelRouteRepository.QueryProvider
                    join t1 in _payChannelRepository.QueryProvider on t0.PayChannelCode equals t1.ChannelCode
                    where String.Compare(t0.AppId, appid, true) == 0
                    select t1).FirstOrDefault();
        }
    }
}
