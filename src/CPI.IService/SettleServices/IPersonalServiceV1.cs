using System;
using System.Collections.Generic;
using System.Text;
using CPI.Common.Domain.SettleDomain.Bill99;
using CPI.Common.Domain.SettleDomain.Bill99.v1_0;
using ATBase.Core;
using ATBase.Core.Collections;

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
        /// 查询开户信息
        /// </summary>
        /// <param name="request"></param>
        XResult<PersonalRegisterInfoQueryResponseV1> QueryPersonalInfo(PersonalRegisterInfoQueryRequestV1 request);
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
        /// <summary>
        /// 申请提现
        /// </summary>
        /// <param name="request"></param>
        XResult<PersonalWithdrawResponseV1> ApplyWithdraw(PersonalWithdrawRequestV1 request);
        /// <summary>
        /// 手机号验证
        /// </summary>
        /// <param name="request"></param>
        XResult<PersonalBindCardSendSmsValidCodeResponseV1> MobileCheck(PersonalBindCardSendSmsValidCodeRequestV1 request);
        /// <summary>
        /// 合同查询
        /// </summary>
        /// <param name="request"></param>
        XResult<PersonalRegisterContractInfoQueryResponseV1> QueryContract(PersonalRegisterContractInfoQueryRequestV1 request);
        /// <summary>
        /// 合同签约
        /// </summary>
        /// <param name="request"></param>
        XResult<PersonalRegisterContractSignResponseV1> SignContract(PersonalRegisterContractSignRequestV1 request);
        /// <summary>
        /// 查询余额
        /// </summary>
        /// <param name="request"></param>
        XResult<PersonalAccountBalanceQueryResponseV1> QueryAccountBalance(PersonalAccountBalanceQueryRequestV1 request);
        /// <summary>
        /// 查询提现单
        /// </summary>
        /// <param name="request"></param>
        XResult<WithdrawOrderQueryResponseV1> QueryWithdrawOrder(WithdrawOrderQueryRequestV1 request);
        /// <summary>
        /// 查询提现单列表
        /// </summary>
        /// <param name="request"></param>
        XResult<WithdrawOrderListQueryResponseV1> QueryWithdrawOrderList(WithdrawOrderListQueryRequestV1 request);
        /// <summary>
        /// 拉取提现结果
        /// </summary>
        /// <param name="request"></param>
        XResult<PersonalWithdrawResultPullResponseV1> PullWithdrawResult(PersonalWithdrawResultPullRequestV1 request);
    }
}
