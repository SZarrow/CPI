using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CPI.Common;
using CPI.Common.Domain.SettleDomain;
using CPI.Common.Domain.SettleDomain.Bill99;
using CPI.Common.Exceptions;
using CPI.Common.Models;
using CPI.Config;
using CPI.Data.PostgreSQL;
using CPI.IData.BaseRepositories;
using CPI.IService.SettleServices;
using CPI.Providers;
using CPI.Utils;
using Lotus.Core;
using Lotus.Logging;

namespace CPI.Services.SettleServices
{
    public class PersonalService : IPersonalService
    {
        private static readonly LockProvider _lockProvider = new LockProvider();
        private static readonly ILogger _logger = LogManager.GetLogger();

        private readonly IPersonalSubAccountRepository _personalSubAccountRepository = null;
        private readonly IWithdrawBankCardBindInfoRepository _withdrawBankCardBindInfoRepository = null;

        public XResult<PersonalCancelBoundCardResponse> CancelBoundCard(PersonalCancelBoundCardRequest request)
        {
            return Bill99UtilYZT.Execute<PersonalCancelBoundCardRequest, PersonalCancelBoundCardResponse>("/person/bankcard/cancel", request);
        }

        public XResult<PersonalInfoQueryResponse> GetAccountInfo(PersonalInfoQueryRequest request)
        {
            if (request == null)
            {
                return new XResult<PersonalInfoQueryResponse>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentNullException(nameof(request)));
            }

            String service = $"{this.GetType().FullName}.GetAccountInfo(...)";

            if (!request.IsValid)
            {
                _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(request)}.IsValid", LogPhase.ACTION, $"请求参数验证失败：{request.ErrorMessage}", request);
                return new XResult<PersonalInfoQueryResponse>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentException(request.ErrorMessage));
            }

            var requestHash = $"getaccountinfo:{request.PayeeId}".GetHashCode();

            if (_lockProvider.Exists(requestHash))
            {
                return new XResult<PersonalInfoQueryResponse>(null, ErrorCode.SUBMIT_REPEAT);
            }

            try
            {
                if (!_lockProvider.Lock(requestHash))
                {
                    return new XResult<PersonalInfoQueryResponse>(null, ErrorCode.SUBMIT_REPEAT);
                }

                var existedRegisterInfo = _personalSubAccountRepository.QueryProvider.FirstOrDefault(x => x.UID == request.PayeeId);
                if (existedRegisterInfo == null)
                {
                    return new XResult<PersonalInfoQueryResponse>(null, SettleErrorCode.UN_REGISTERED);
                }

                if (existedRegisterInfo.Status != PersonalInfoRegisterStatus.SUCCESS.ToString())
                {
                    var queryResult = Bill99UtilYZT.Execute<RawPersonalInfoQueryRequest, RawPersonalInfoQueryResponse>("/person/info", new RawPersonalInfoQueryRequest()
                    {
                        PayeeId = request.PayeeId
                    });

                    if (queryResult.Success && queryResult.Value != null && queryResult.Value.AuditStatus == "03")
                    {
                        existedRegisterInfo.Status = PersonalInfoRegisterStatus.SUCCESS.ToString();
                        _personalSubAccountRepository.Update(existedRegisterInfo);
                        var updateStatusResult = _personalSubAccountRepository.SaveChanges();
                        if (!updateStatusResult.Success)
                        {
                            _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(_personalSubAccountRepository)}.SaveChanges()", "更新开户状态失败", updateStatusResult.FirstException, existedRegisterInfo);
                        }
                    }
                }

                return new XResult<PersonalInfoQueryResponse>(new PersonalInfoQueryResponse()
                {
                    PayeeId = existedRegisterInfo.UID,
                    Email = existedRegisterInfo.Email,
                    IDCardNo = existedRegisterInfo.IDCardNo,
                    IDCardType = existedRegisterInfo.IDCardType,
                    Mobile = existedRegisterInfo.Mobile,
                    RealName = existedRegisterInfo.RealName,
                    Status = existedRegisterInfo.Status,
                    Msg = GetRegisterAuditStatusMsg(existedRegisterInfo.Status)
                });
            }
            finally
            {
                _lockProvider.UnLock(requestHash);
            }
        }

        public XResult<PersonalBoundCardListQueryResponse> GetBoundCards(PersonalBoundCardListQueryRequest request)
        {
            if (request == null)
            {
                return new XResult<PersonalBoundCardListQueryResponse>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentNullException(nameof(request)));
            }

            String service = $"{this.GetType().FullName}.GetBoundCards(...)";

            if (!request.IsValid)
            {
                return new XResult<PersonalBoundCardListQueryResponse>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentException(request.ErrorMessage));
            }

            var requestHash = $"getboundcards:{request.PayeeId}".GetHashCode();

            if (_lockProvider.Exists(requestHash))
            {
                return new XResult<PersonalBoundCardListQueryResponse>(null, ErrorCode.SUBMIT_REPEAT);
            }

            try
            {
                if (!_lockProvider.Lock(requestHash))
                {
                    return new XResult<PersonalBoundCardListQueryResponse>(null, ErrorCode.SUBMIT_REPEAT);
                }

                var existedBoundInfos = _withdrawBankCardBindInfoRepository.QueryProvider.Where(x => x.PayeeId == request.PayeeId).ToList();
                if (existedBoundInfos == null || existedBoundInfos.Count == 0)
                {
                    var queryResult = Bill99UtilYZT.Execute<RawPersonalBoundCardListQueryRequest, RawPersonalBoundCardListQueryResponse>("/person/bankcard/list", new RawPersonalBoundCardListQueryRequest()
                    {
                        PayeeId = request.PayeeId
                    });

                    if (queryResult.Success && queryResult.Value != null && queryResult.Value.ResponseCode == "0000")
                    {
                        var bindcards = queryResult.Value.BindCards;
                        if (bindcards != null && bindcards.Count() > 0)
                        {
                            _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, "existedBoundInfos", "数据库中没有绑卡信息但快钱返回有绑卡信息", null, new Object[] { request, bindcards });
                        }
                    }

                    return new XResult<PersonalBoundCardListQueryResponse>(null, ErrorCode.INFO_NOT_EXIST, new RemoteException("绑卡信息不存在"));
                }

                //获取既不是成功也不是失败的绑卡信息，然后查快钱接口更新它们
                var unConfirmBoundInfos = existedBoundInfos.Where(x => x.BindStatus != WithdrawBindCardStatus.SUCCESS.ToString() && x.BindStatus != WithdrawBindCardStatus.FAILURE.ToString());
                if (unConfirmBoundInfos.Count() > 0)
                {
                    var queryResult = Bill99UtilYZT.Execute<RawPersonalBoundCardListQueryRequest, RawPersonalBoundCardListQueryResponse>("/person/bankcard/list", new RawPersonalBoundCardListQueryRequest()
                    {
                        PayeeId = request.PayeeId
                    });

                    if (queryResult.Success && queryResult.Value != null && queryResult.Value.ResponseCode == "0000")
                    {
                        var bindcards = queryResult.Value.BindCards;
                        if (bindcards != null && bindcards.Count() > 0)
                        {

                            var joined = from t0 in bindcards
                                         join t1 in unConfirmBoundInfos
                                         on t0.BankCardNo equals t1.BankCardNo
                                         where t0.Status == "1" || t0.Status == "9"
                                         select new
                                         {
                                             LocalBoundInfo = t1,
                                             RemoteBoundInfo = t0
                                         };

                            Boolean statusHasChanged = false;

                            if (joined.Count() > 0)
                            {
                                foreach (var item in joined)
                                {
                                    //状态为1，绑卡成功
                                    if (item.RemoteBoundInfo.Status == "1")
                                    {
                                        item.LocalBoundInfo.BindStatus = WithdrawBindCardStatus.SUCCESS.ToString();
                                        statusHasChanged = true;
                                    }
                                    else
                                    {
                                        //状态为9，绑卡失败，删除绑卡记录
                                        _withdrawBankCardBindInfoRepository.Remove(item.LocalBoundInfo);
                                        existedBoundInfos.Remove(item.LocalBoundInfo);
                                        statusHasChanged = true;
                                    }
                                }
                            }

                            if (statusHasChanged)
                            {
                                var updateStatusResult = _withdrawBankCardBindInfoRepository.SaveChanges();
                                if (!updateStatusResult.Success)
                                {
                                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(_withdrawBankCardBindInfoRepository)}.SaveChanges()", "更新绑卡状态失败", updateStatusResult.FirstException, existedBoundInfos);
                                }
                            }
                        }
                    }
                }

                return new XResult<PersonalBoundCardListQueryResponse>(new PersonalBoundCardListQueryResponse()
                {
                    BindCards = existedBoundInfos
                });
            }
            finally
            {
                _lockProvider.UnLock(requestHash);
            }
        }

        public XResult<WithdrawBindCardQueryStatusResponse> QueryBindCardStatus(WithdrawBindCardQueryStatusRequest request)
        {
            if (request == null)
            {
                return new XResult<WithdrawBindCardQueryStatusResponse>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentNullException(nameof(request)));
            }

            String service = $"{this.GetType().FullName}.QueryBindCardStatus(...)";

            if (!request.IsValid)
            {
                return new XResult<WithdrawBindCardQueryStatusResponse>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentException(request.ErrorMessage));
            }

            var requestHash = $"querybindcardstatus:{request.PayeeId}.{request.BankCardNo}".GetHashCode();

            if (_lockProvider.Exists(requestHash))
            {
                return new XResult<WithdrawBindCardQueryStatusResponse>(null, ErrorCode.SUBMIT_REPEAT);
            }

            try
            {
                if (!_lockProvider.Lock(requestHash))
                {
                    return new XResult<WithdrawBindCardQueryStatusResponse>(null, ErrorCode.SUBMIT_REPEAT);
                }

                var queryResult = Bill99UtilYZT.Execute<RawWithdrawBindCardQueryStatusRequest, RawWithdrawBindCardQueryStatusResponse>("/bankacct/queryStatus", new RawWithdrawBindCardQueryStatusRequest()
                {
                    uId = request.PayeeId,
                    bankAcctId = request.BankCardNo
                });

                if (!queryResult.Success)
                {
                    return new XResult<WithdrawBindCardQueryStatusResponse>(null, queryResult.ErrorCode, queryResult.FirstException);
                }

                if (queryResult.Value == null)
                {
                    return new XResult<WithdrawBindCardQueryStatusResponse>(null, ErrorCode.REMOTE_RETURN_NOTHING, new RemoteException("快钱未返回任何数据"));
                }

                if (queryResult.Value.ResponseCode != "0000")
                {
                    return new XResult<WithdrawBindCardQueryStatusResponse>(null, ErrorCode.FAILURE, new RemoteException($"{queryResult.Value.ResponseCode}:{queryResult.Value.ResponseMessage}"));
                }

                var bindcards = queryResult.Value.BindCards;
                if (bindcards == null || bindcards.Count() == 0)
                {
                    return new XResult<WithdrawBindCardQueryStatusResponse>(null, SettleErrorCode.NO_BANKCARD_BOUND);
                }

                var first = bindcards.FirstOrDefault();

                String status = WithdrawBindCardStatus.PROCESSING.ToString();
                switch (first.Status)
                {
                    case "1":
                        status = WithdrawBindCardStatus.SUCCESS.ToString();
                        break;
                    case "9":
                        status = WithdrawBindCardStatus.FAILURE.ToString();
                        break;
                }

                return new XResult<WithdrawBindCardQueryStatusResponse>(new WithdrawBindCardQueryStatusResponse()
                {
                    BankCardNo = first.BankCardNo,
                    MemberBankAcctId = first.MemberBankAcctId,
                    Status = status,
                    Msg = GetBindCardStatusMsg(status)
                });
            }
            finally
            {
                _lockProvider.UnLock(requestHash);
            }
        }

        public XResult<PersonalRegisterResponse> Register(PersonalRegisterRequest request)
        {
            if (request == null)
            {
                return new XResult<PersonalRegisterResponse>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentNullException(nameof(request)));
            }

            String service = $"{this.GetType().FullName}.Register(...)";

            if (!request.IsValid)
            {
                _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(request)}.IsValid", LogPhase.ACTION, $"请求参数验证失败：{request.ErrorMessage}", request);
                return new XResult<PersonalRegisterResponse>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentException(request.ErrorMessage));
            }

            var requestHash = $"register:{request.PayeeId}".GetHashCode();

            if (_lockProvider.Exists(requestHash))
            {
                return new XResult<PersonalRegisterResponse>(null, ErrorCode.SUBMIT_REPEAT);
            }

            try
            {
                if (!_lockProvider.Lock(requestHash))
                {
                    return new XResult<PersonalRegisterResponse>(null, ErrorCode.SUBMIT_REPEAT);
                }

                var existedRegisterInfo = _personalSubAccountRepository.QueryProvider.Where(x => x.AppId == request.AppId && x.UID == request.PayeeId).Count() > 0;
                if (existedRegisterInfo)
                {
                    return new XResult<PersonalRegisterResponse>(null, ErrorCode.INFO_EXISTED, new RequestException("开户信息已存在"));
                }

                var newId = IDGenerator.GenerateID();
                var newAccount = new PersonalSubAccount()
                {
                    Id = newId,
                    AppId = request.AppId,
                    UID = request.PayeeId,
                    IDCardNo = request.IDCardNo,
                    IDCardType = request.IDCardType,
                    RealName = request.RealName,
                    Mobile = request.Mobile,
                    Email = request.Email,
                    Status = PersonalInfoRegisterStatus.WAITFORAUDIT.ToString(),
                    UpdateTime = DateTime.Now
                };
                _personalSubAccountRepository.Add(newAccount);

                var saveResult = _personalSubAccountRepository.SaveChanges();
                if (!saveResult.Success)
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(_personalSubAccountRepository)}.SaveChanges()", "保存个人开户信息失败", saveResult.FirstException, request);
                    return new XResult<PersonalRegisterResponse>(null, ErrorCode.DB_UPDATE_FAILED, saveResult.FirstException);
                }

                String traceMethod = "Bill99Util.Execute(/person/register)";

                _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.OK.ToString(), service, traceMethod, LogPhase.BEGIN, "开始调用快钱个人开户接口", request);

                var execResult = Bill99UtilYZT.Execute<RawPersonalRegisterRequest, RawPersonalRegisterResponse>("/person/register", new RawPersonalRegisterRequest()
                {
                    uId = request.PayeeId,
                    email = request.Email,
                    idCardNumber = request.IDCardNo,
                    idCardType = request.IDCardType,
                    mobile = request.Mobile,
                    name = request.RealName
                });

                _logger.Trace(TraceType.BLL.ToString(), (execResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), service, traceMethod, LogPhase.END, $"结束调用快钱开户接口", request);

                if (!execResult.Success || execResult.Value == null)
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, traceMethod, "个人开户失败", execResult.FirstException, execResult.Value);

                    _personalSubAccountRepository.Remove(newAccount);
                    saveResult = _personalSubAccountRepository.SaveChanges();
                    if (!saveResult.Success)
                    {
                        _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, traceMethod, "删除个人开户记录失败", saveResult.FirstException, newAccount);
                    }

                    _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.OK.ToString(), service, traceMethod, LogPhase.ACTION, "已删除开户失败的记录");
                    return new XResult<PersonalRegisterResponse>(null, ErrorCode.DEPENDENT_API_CALL_FAILED, execResult.FirstException);
                }

                //开户成功之后要更新openid
                if (execResult.Value.ResponseCode == "0000")
                {
                    Boolean statusHasChanged = false;
                    switch (execResult.Value.AuditStatus)
                    {
                        case "03":
                            newAccount.Status = PersonalInfoRegisterStatus.SUCCESS.ToString();
                            newAccount.OpenId = execResult.Value.OpenId;
                            statusHasChanged = true;
                            break;
                        case "02":
                            newAccount.Status = PersonalInfoRegisterStatus.WAITFORREVIEW.ToString();
                            statusHasChanged = true;
                            break;
                    }

                    if (statusHasChanged)
                    {
                        _personalSubAccountRepository.Update(newAccount);
                        var updateResult = _personalSubAccountRepository.SaveChanges();
                        if (!updateResult.Success)
                        {
                            _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, traceMethod, "更新开户信息的OpenId失败", updateResult.FirstException, newAccount);
                            return new XResult<PersonalRegisterResponse>(null, ErrorCode.DB_UPDATE_FAILED, new DbUpdateException("更新开户信息失败"));
                        }
                    }
                }

                var resp = new PersonalRegisterResponse()
                {
                    Status = newAccount.Status,
                    OpenId = newAccount.OpenId,
                    Msg = GetRegisterAuditStatusMsg(newAccount.Status)
                };

                return new XResult<PersonalRegisterResponse>(resp);
            }
            finally
            {
                _lockProvider.UnLock(requestHash);
            }
        }

        private String GetRegisterAuditStatusMsg(String status)
        {
            switch (status)
            {
                case "WAITFORAUDIT":
                    return PersonalInfoRegisterStatus.WAITFORAUDIT.GetDescription();
                case "WAITFORREVIEW":
                    return PersonalInfoRegisterStatus.WAITFORREVIEW.GetDescription();
                case "SUCCESS":
                    return PersonalInfoRegisterStatus.SUCCESS.GetDescription();
            }

            return String.Empty;
        }

        private String GetBindCardStatusMsg(String status)
        {
            switch (status)
            {
                case "PROCESSING":
                    return WithdrawBindCardStatus.PROCESSING.GetDescription();
                case "WAITFORAUDIT":
                    return WithdrawBindCardStatus.WAITFORAUDIT.GetDescription();
                case "SUCCESS":
                    return WithdrawBindCardStatus.SUCCESS.GetDescription();
                case "FAILURE":
                    return WithdrawBindCardStatus.FAILURE.GetDescription();
            }

            return String.Empty;
        }

        public XResult<PersonalInfoUpdateResponse> UpdateAccountInfo(PersonalInfoUpdateRequest request)
        {
            if (request == null)
            {
                return new XResult<PersonalInfoUpdateResponse>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentNullException(nameof(request)));
            }

            if (!request.IsValid)
            {
                return new XResult<PersonalInfoUpdateResponse>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentException(request.ErrorMessage));
            }

            String service = $"{this.GetType().FullName}.UpdateAccountInfo(...)";

            var requestHash = $"update:{request.UID}".GetHashCode();

            if (_lockProvider.Exists(requestHash))
            {
                return new XResult<PersonalInfoUpdateResponse>(null, ErrorCode.SUBMIT_REPEAT);
            }

            try
            {
                if (!_lockProvider.Lock(requestHash))
                {
                    return new XResult<PersonalInfoUpdateResponse>(null, ErrorCode.SUBMIT_REPEAT);
                }

                var find = _personalSubAccountRepository.QueryProvider.FirstOrDefault(x => x.UID == request.UID);
                if (find == null)
                {
                    return new XResult<PersonalInfoUpdateResponse>(null, ErrorCode.INFO_NOT_EXIST, new ArgumentException("开户信息不存在"));
                }

                Boolean hasChanged = false;

                if (String.Compare(find.Mobile, request.Mobile, true) != 0)
                {
                    find.Mobile = request.Mobile;
                    hasChanged = true;
                }

                if (String.Compare(find.Email, request.Email, true) != 0)
                {
                    find.Email = request.Email;
                    hasChanged = true;
                }

                if (!hasChanged)
                {
                    return new XResult<PersonalInfoUpdateResponse>(new PersonalInfoUpdateResponse()
                    {
                        Status = CommonStatus.SUCCESS.ToString(),
                        Msg = CommonStatus.SUCCESS.GetDescription()
                    });
                }

                String traceMethod = "Bill99Util.Execute(/person/updateMember)";

                _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.OK.ToString(), service, traceMethod, LogPhase.BEGIN, "开始更新个人账户信息", request);

                var execResult = Bill99UtilYZT.Execute<PersonalInfoUpdateRequest, RawPersonalInfoUpdateResponse>("/person/updateMember", request);

                _logger.Trace(TraceType.BLL.ToString(), (execResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), service, traceMethod, LogPhase.END, "结束更新个人账户信息", execResult.Value);

                if (!execResult.Success)
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, traceMethod, "更新个人账户信息失败", execResult.FirstException, request);
                    return new XResult<PersonalInfoUpdateResponse>(new PersonalInfoUpdateResponse()
                    {
                        Status = CommonStatus.FAILURE.ToString(),
                        Msg = CommonStatus.FAILURE.GetDescription()
                    });
                }

                _personalSubAccountRepository.Update(find);
                var saveResult = _personalSubAccountRepository.SaveChanges();
                if (!saveResult.Success)
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, traceMethod, "更新个人账户信息失败", saveResult.FirstException, request);
                    return new XResult<PersonalInfoUpdateResponse>(null, saveResult.FirstException);
                }

                return new XResult<PersonalInfoUpdateResponse>(new PersonalInfoUpdateResponse()
                {
                    Status = CommonStatus.SUCCESS.ToString(),
                    Msg = CommonStatus.SUCCESS.GetDescription()
                });
            }
            finally
            {
                _lockProvider.UnLock(requestHash);
            }
        }

        public XResult<PersonalWithdrawBindCardResponse> WithdrawBindCard(PersonalWithdrawBindCardRequest request)
        {
            if (request == null)
            {
                return new XResult<PersonalWithdrawBindCardResponse>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentNullException(nameof(request)));
            }

            String service = $"{this.GetType().FullName}.WithdrawBindCard(...)";

            if (!request.IsValid)
            {
                _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(request)}.IsValid", LogPhase.ACTION, $"请求参数验证失败：{request.ErrorMessage}", request);
                return new XResult<PersonalWithdrawBindCardResponse>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentNullException(request.ErrorMessage));
            }

            var requestHash = $"bindcard:{request.PayeeId}".GetHashCode();

            if (_lockProvider.Exists(requestHash))
            {
                return new XResult<PersonalWithdrawBindCardResponse>(null, ErrorCode.SUBMIT_REPEAT);
            }

            try
            {
                if (!_lockProvider.Lock(requestHash))
                {
                    return new XResult<PersonalWithdrawBindCardResponse>(null, ErrorCode.SUBMIT_REPEAT);
                }

                var existedBindInfo = _withdrawBankCardBindInfoRepository.Exists(x => x.PayeeId == request.PayeeId && x.BankCardNo == request.BankCardNo);
                if (existedBindInfo)
                {
                    return new XResult<PersonalWithdrawBindCardResponse>(null, ErrorCode.INFO_EXISTED, new ArgumentException("绑卡信息已存在"));
                }

                var newBindInfo = new WithdrawBankCardBindInfo()
                {
                    Id = IDGenerator.GenerateID(),
                    AppId = request.AppId,
                    PayeeId = request.PayeeId,
                    BankCardNo = request.BankCardNo,
                    Mobile = request.Mobile,
                    ApplyTime = DateTime.Now,
                    BankCardFlag = request.SecondAccountFlag.ToInt32(),
                    BindStatus = WithdrawBindCardStatus.PROCESSING.ToString()
                };

                using (var tx = new TransactionScope())
                {
                    _withdrawBankCardBindInfoRepository.Add(newBindInfo);
                    var saveResult = _withdrawBankCardBindInfoRepository.SaveChanges();
                    if (!saveResult.Success)
                    {
                        _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(_withdrawBankCardBindInfoRepository)}.SaveChanges()", "保存提现绑卡信息失败", saveResult.FirstException, request);
                        return new XResult<PersonalWithdrawBindCardResponse>(null, ErrorCode.DB_UPDATE_FAILED, saveResult.FirstException);
                    }

                    String traceMethod = "Bill99Util.Execute(/person/bankcard/bind)";

                    _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.OK.ToString(), service, traceMethod, LogPhase.BEGIN);

                    var execResult = Bill99UtilYZT.Execute<RawPersonalWithdrawBindCardRequest, RawPersonalWithdrawBindCardResponse>("/person/bankcard/bind", new RawPersonalWithdrawBindCardRequest()
                    {
                        uId = request.PayeeId,
                        bankAcctId = request.BankCardNo,
                        mobile = request.Mobile,
                        secondAcct = request.SecondAccountFlag
                    });

                    _logger.Trace(TraceType.BLL.ToString(), (execResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), service, traceMethod, LogPhase.END);

                    if (!execResult.Success || execResult.Value == null)
                    {
                        _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, traceMethod, "提现绑卡失败", execResult.FirstException, request);
                        return new XResult<PersonalWithdrawBindCardResponse>(null, ErrorCode.DEPENDENT_API_CALL_FAILED, execResult.FirstException);
                    }

                    if (execResult.Value.ResponseCode != "0000")
                    {
                        _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, traceMethod, "提现绑卡失败", null, execResult.Value);
                        return new XResult<PersonalWithdrawBindCardResponse>(null, ErrorCode.DEPENDENT_API_CALL_FAILED, new RemoteException(execResult.Value.ResponseMessage));
                    }

                    newBindInfo.MemberBankAccountId = execResult.Value.memberBankAcctId;
                    var updateResult = _withdrawBankCardBindInfoRepository.SaveChanges();
                    if (!updateResult.Success)
                    {
                        _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, traceMethod, "更新绑卡信息失败", updateResult.FirstException, newBindInfo);
                        return new XResult<PersonalWithdrawBindCardResponse>(null, ErrorCode.DB_UPDATE_FAILED, new RequestException("更新绑卡信息失败"));
                    }

                    tx.Complete();
                }

                var resp = new PersonalWithdrawBindCardResponse()
                {
                    MemberBankAccountId = newBindInfo.MemberBankAccountId,
                    Status = CommonStatus.SUCCESS.ToString(),
                    Msg = $"绑卡{CommonStatus.SUCCESS.GetDescription()}"
                };

                return new XResult<PersonalWithdrawBindCardResponse>(resp);
            }
            finally
            {
                _lockProvider.UnLock(requestHash);
            }
        }

        public XResult<PersonalWithdrawRebindCardResponse> WithdrawRebindCard(PersonalWithdrawRebindCardRequest request)
        {
            if (request == null)
            {
                return new XResult<PersonalWithdrawRebindCardResponse>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentNullException(nameof(request)));
            }

            String service = $"{this.GetType().FullName}.WithdrawRebindCard(...)";

            if (!request.IsValid)
            {
                _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(request)}.IsValid", LogPhase.ACTION, $"请求参数验证失败：{request.ErrorMessage}", request);
                return new XResult<PersonalWithdrawRebindCardResponse>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentException(request.ErrorMessage));
            }

            var requestHash = $"rebindcard:{request.PayeeId}".GetHashCode();

            if (_lockProvider.Exists(requestHash))
            {
                return new XResult<PersonalWithdrawRebindCardResponse>(null, ErrorCode.SUBMIT_REPEAT);
            }

            try
            {
                if (!_lockProvider.Lock(requestHash))
                {
                    return new XResult<PersonalWithdrawRebindCardResponse>(null, ErrorCode.SUBMIT_REPEAT);
                }

                var existedBindInfo = _withdrawBankCardBindInfoRepository.QueryProvider.FirstOrDefault(x => x.PayeeId == request.PayeeId);
                if (existedBindInfo == null)
                {
                    return new XResult<PersonalWithdrawRebindCardResponse>(null, ErrorCode.DB_QUERY_FAILED, new DbQueryException("未查询到用户的绑卡信息"));
                }

                Boolean hasChanged = false;

                if (String.Compare(existedBindInfo.BankCardNo, request.BankCardNo, true) != 0)
                {
                    existedBindInfo.BankCardNo = request.BankCardNo;
                    hasChanged = true;
                }

                Int32 secondAccountFlag = request.SecondAccountFlag.ToInt32();
                if (existedBindInfo.BankCardFlag != secondAccountFlag)
                {
                    existedBindInfo.BankCardFlag = secondAccountFlag;
                    hasChanged = true;
                }

                if (String.Compare(existedBindInfo.Mobile, request.Mobile, true) != 0)
                {
                    existedBindInfo.Mobile = request.Mobile;
                    hasChanged = true;
                }

                if (!hasChanged)
                {
                    return new XResult<PersonalWithdrawRebindCardResponse>(new PersonalWithdrawRebindCardResponse()
                    {
                        MemberBankAccountId = existedBindInfo.MemberBankAccountId,
                        Status = CommonStatus.SUCCESS.ToString(),
                        Msg = CommonStatus.SUCCESS.GetDescription()
                    });
                }

                String traceMethod = $"Bill99Util.Execute(/person/bankcard/rebind)";

                _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.OK.ToString(), service, traceMethod, LogPhase.BEGIN);

                var execResult = Bill99UtilYZT.Execute<RawPersonalWithdrawRebindCardRequest, RawPersonalWithdrawRebindCardResponse>("/person/bankcard/rebind", new RawPersonalWithdrawRebindCardRequest()
                {
                    bankAcctId = request.BankCardNo,
                    mobile = request.Mobile,
                    secondAcct = request.SecondAccountFlag,
                    uId = request.PayeeId
                });

                _logger.Trace(TraceType.BLL.ToString(), (execResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), service, traceMethod, LogPhase.END);

                if (!execResult.Success || execResult.Value == null)
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, traceMethod, "变更绑卡失败", execResult.FirstException, request);
                    return new XResult<PersonalWithdrawRebindCardResponse>(null, ErrorCode.DEPENDENT_API_CALL_FAILED, new RemoteException("变更绑卡失败"));
                }

                var rebindStatus = CommonStatus.FAILURE;

                if (execResult.Value.ResponseCode == "0000")
                {
                    existedBindInfo.MemberBankAccountId = execResult.Value.MemberBankAccountId;
                    var updateResult = _withdrawBankCardBindInfoRepository.SaveChanges();
                    if (!updateResult.Success)
                    {
                        _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(_withdrawBankCardBindInfoRepository)}.SaveChanges()", "保存变更绑卡信息失败", updateResult.FirstException, existedBindInfo);
                    }

                    rebindStatus = CommonStatus.SUCCESS;
                }

                return new XResult<PersonalWithdrawRebindCardResponse>(new PersonalWithdrawRebindCardResponse()
                {
                    Status = rebindStatus.ToString(),
                    Msg = $"换卡{rebindStatus.GetDescription()}"
                });
            }
            finally
            {
                _lockProvider.UnLock(requestHash);
            }
        }

        //public XResult<Int32> PullRegisterAuditResult(Int32 count = 20)
        //{
        //    if (count <= 0)
        //    {
        //        return new XResult<Int32>(0);
        //    }

        //    String service = $"{this.GetType().FullName}:PullRegisterAuditResult()";

        //    var hashKey = $"pullregisterresult:{DateTime.Now.ToString("yyMMddHH")}".GetHashCode();

        //    if (_lockProvider.Exists(hashKey))
        //    {
        //        return new XResult<Int32>(0, new RequestException("重复提交"));
        //    }

        //    try
        //    {
        //        if (!_lockProvider.Lock(hashKey))
        //        {
        //            return new XResult<Int32>(0, new RequestException("重复提交"));
        //        }

        //        var personalInfos = (from t0 in _personalSubAccountRepository.QueryProvider
        //                             where t0.Status != PersonalInfoRegisterStatus.SUCCESS.ToString()
        //                             orderby t0.UpdateTime
        //                             select t0).Take(count).ToList();

        //        if (personalInfos == null || personalInfos.Count == 0)
        //        {
        //            return new XResult<Int32>(0);
        //        }

        //        var c_personalInfos = new ConcurrentStack<PersonalSubAccount>(personalInfos);
        //        var tasks = new List<Task>(personalInfos.Count);
        //        var successCount = 0;

        //        while (c_personalInfos.Count > 0)
        //        {
        //            tasks.Add(Task.Run(() =>
        //            {
        //                if (!c_personalInfos.TryPop(out PersonalSubAccount personalInfo)) { return; }

        //                var request = new PersonalInfoQueryRequest()
        //                {
        //                    PayeeId = personalInfo.UID
        //                };

        //                var respResult = GetAccountInfo(request);
        //                if (!respResult.Success || respResult.Value == null)
        //                {
        //                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, "GetAccountInfo(...)", "查询个人账户信息失败", respResult.FirstException, request);
        //                    return;
        //                }

        //                if (respResult.Value.ResponseCode == "0000")
        //                {
        //                    Boolean statusHasChanged = false;
        //                    switch (respResult.Value.AuditStatus)
        //                    {
        //                        case "03":
        //                            personalInfo.Status = PersonalInfoRegisterStatus.SUCCESS.ToString();
        //                            personalInfo.UpdateTime = DateTime.Now;
        //                            statusHasChanged = true;
        //                            break;
        //                    }

        //                    if (statusHasChanged)
        //                    {
        //                        var updateResult = _personalSubAccountRepository.SaveChanges();
        //                        if (!updateResult.Success)
        //                        {
        //                            _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(_personalSubAccountRepository)}.SaveChanges()", "更新个人账户审核状态失败", updateResult.FirstException, personalInfo);
        //                            return;
        //                        }
        //                    }

        //                    Interlocked.Increment(ref successCount);
        //                }
        //                else
        //                {
        //                    if ((DateTime.Now - personalInfo.UpdateTime).TotalDays > 7)
        //                    {
        //                        _personalSubAccountRepository.Remove(personalInfo);
        //                        var updateResult = _personalSubAccountRepository.SaveChanges();
        //                        if (!updateResult.Success)
        //                        {
        //                            _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(_personalSubAccountRepository)}.SaveChanges()", "删除审核失败的个人账户信息失败", updateResult.FirstException, personalInfo);
        //                            return;
        //                        }

        //                        Interlocked.Increment(ref successCount);
        //                    }
        //                }
        //            }));
        //        }

        //        try
        //        {
        //            Task.WaitAll(tasks.ToArray());
        //            return new XResult<Int32>(successCount);
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, "Task.WaitAll(...)", "拉取分账结果的任务出现异常", ex);
        //            return new XResult<Int32>(successCount, ErrorCode.TASK_EXECUTE_FAILED, ex);
        //        }
        //    }
        //    finally
        //    {
        //        _lockProvider.UnLock(hashKey);
        //    }
        //}
    }
}
