using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPI.Common;
using CPI.IData.BaseRepositories;
using CPI.IService.BaseServices;
using Lotus.Logging;

namespace CPI.Services.BaseServices
{
    public class SysAppService : ISysAppService
    {
        private static readonly Dictionary<String, String> _rsaPublicKeyCache = new Dictionary<String, String>(10);
        private static readonly ILogger _logger = LogManager.GetLogger();

        private readonly ISysAppRepository _sysAppRepository = null;

        public String GetRSAPublicKey(String appid)
        {
            if (String.IsNullOrWhiteSpace(appid))
            {
                return null;
            }

            if (_rsaPublicKeyCache.ContainsKey(appid))
            {
                return _rsaPublicKeyCache[appid];
            }

            String service = $"{this.GetType().FullName}:GetRSAPublicKey()";

            try
            {
                String key = _sysAppRepository.QueryProvider.Where(x => x.AppId == appid).Select(x => x.AppRSAPublicKey).FirstOrDefault();
                if (!String.IsNullOrWhiteSpace(key))
                {
                    _rsaPublicKeyCache[appid] = key;
                }

                return key;
            }
            catch (Exception ex)
            {
                _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, ":", "查询应用系统的RSA公钥失败", ex, new
                {
                    AppId = appid
                });
                return null;
            }
        }
    }
}
