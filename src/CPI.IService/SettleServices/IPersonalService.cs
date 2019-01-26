using System;
using System.Collections.Generic;
using System.Text;
using CPI.Common.Domain.SettleDomain.Bill99;
using Lotus.Core;

namespace CPI.IService.SettleServices
{
    /// <summary>
    /// 个人账户服务接口
    /// </summary>
    public interface IPersonalService
    {
        /// <summary>
        /// 分账开户
        /// </summary>
        /// <param name="request">开户请求参数</param>
        XResult<PersonalRegisterResponse> Register(PersonalRegisterRequest request);
        /// <summary>
        /// 更新个人账户信息
        /// </summary>
        /// <param name="request">更新请求参数</param>
        XResult<PersonalInfoUpdateResponse> UpdateAccountInfo(PersonalInfoUpdateRequest request);
        /// <summary>
        /// 获取个人账户信息
        /// </summary>
        /// <param name="request"></param>
        XResult<PersonalInfoQueryResponse> GetAccountInfo(PersonalInfoQueryRequest request);
        /// <summary>
        /// 查询绑卡状态
        /// </summary>
        /// <param name="request"></param>
        XResult<WithdrawBindCardQueryStatusResponse> QueryBindCardStatus(WithdrawBindCardQueryStatusRequest request);
        /// <summary>
        /// 提现绑卡
        /// </summary>
        /// <param name="request">绑卡请求参数</param>
        XResult<PersonalWithdrawBindCardResponse> WithdrawBindCard(PersonalWithdrawBindCardRequest request);
        /// <summary>
        /// 提现重新绑卡
        /// </summary>
        /// <param name="request">重新绑卡请求参数</param>
        XResult<PersonalWithdrawRebindCardResponse> WithdrawRebindCard(PersonalWithdrawRebindCardRequest request);
        /// <summary>
        /// 取消绑卡
        /// </summary>
        /// <param name="request">取消绑卡请求参数</param>
        XResult<PersonalCancelBoundCardResponse> CancelBoundCard(PersonalCancelBoundCardRequest request);
        /// <summary>
        /// 获取个人账户已绑定的银行卡列表
        /// </summary>
        /// <param name="request">绑卡列表查询请求参数</param>
        XResult<PersonalBoundCardListQueryResponse> GetBoundCards(PersonalBoundCardListQueryRequest request);
        ///// <summary>
        ///// 拉取注册结果
        ///// </summary>
        ///// <param name="count"></param>
        //XResult<Int32> PullRegisterAuditResult(Int32 count = 20);
    }
}
