using System;
using System.Collections.Generic;
using System.Text;
using CPI.Common.Domain.SettleDomain.Bill99;
using CPI.Common.Domain.SettleDomain.Bill99.v1_0;
using Lotus.Core;

namespace CPI.IService.SettleServices
{
    /// <summary>
    /// 个人账户服务接口
    /// </summary>
    public interface IPersonalServiceV1
    {
        /// <summary>
        /// 分账开户
        /// </summary>
        /// <param name="request">开户请求参数</param>
        XResult<PersonalRegisterResponseV1> Register(PersonalRegisterRequestV1 request);
        /// <summary>
        /// 查询银行卡受理能力
        /// </summary>
        /// <param name="request"></param>
        XResult<QueryBankCardAcceptResponseV1> QueryBankCardAccept(QueryBankCardAcceptRequestV1 request);
        /// <summary>
        /// 申请绑卡
        /// </summary>
        /// <param name="request"></param>
        XResult<PersonalApplyBindCardResponseV1> ApplyBindCard(PersonalApplyBindCardRequestV1 request);
        /// <summary>
        /// 提现绑卡
        /// </summary>
        /// <param name="request">绑卡请求参数</param>
        XResult<PersonalWithdrawBindCardResponseV1> WithdrawBindCard(PersonalWithdrawBindCardRequestV1 request);
    }
}
