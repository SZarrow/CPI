using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.IService.BaseServices
{
    /// <summary>
    /// 应用系统基本信息服务接口
    /// </summary>
    public interface ISysAppService
    {
        /// <summary>
        /// 根据AppId获取RSA公钥
        /// </summary>
        /// <param name="appid">CPI分配给应用系统的Id</param>
        String GetRSAPublicKey(String appid);
    }
}
