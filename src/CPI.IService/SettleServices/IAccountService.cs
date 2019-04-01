using System;
using System.Collections.Generic;
using System.Text;
using CPI.Common.Domain.SettleDomain.Bill99;
using ATBase.Core;

namespace CPI.IService.SettleServices
{
    /// <summary>
    /// 账户服务接口
    /// </summary>
    public interface IAccountService
    {
        /// <summary>
        /// 查询账户余额
        /// </summary>
        /// <param name="request">账户余额查询请求参数</param>
        XResult<AccountBalanceQueryResponse> GetBalance(AccountBalanceQueryRequest request);
    }
}
