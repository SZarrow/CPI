using System;
using System.Linq;
using CPI.Common;
using CPI.Common.Domain.SettleDomain.Bill99;
using CPI.Common.Domain.SettleDomain.Bill99.v1_0;
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
    public class PersonalServiceV1 : IPersonalServiceV1
    {
        private static readonly LockProvider _lockProvider = new LockProvider();
        private static readonly ILogger _logger = LogManager.GetLogger();

        private readonly IPersonalSubAccountRepository _personalSubAccountRepository = null;
        private readonly IWithdrawBankCardBindInfoRepository _withdrawBankCardBindInfoRepository = null;

        public XResult<PersonalRegisterResponseV1> Register(PersonalRegisterRequestV1 request)
        {
            if (request == null)
            {
                return new XResult<PersonalRegisterResponseV1>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentNullException(nameof(request)));
            }

            String service = $"{this.GetType().FullName}.Register(...)";

            if (!request.IsValid)
            {
                _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(request)}.IsValid", LogPhase.ACTION, $"请求参数验证失败：{request.ErrorMessage}", request);
                return new XResult<PersonalRegisterResponseV1>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentException(request.ErrorMessage));
            }

            var requestHash = $"register:{request.UserId}".GetHashCode();

            if (_lockProvider.Exists(requestHash))
            {
                return new XResult<PersonalRegisterResponseV1>(null, ErrorCode.SUBMIT_REPEAT);
            }

            try
            {
                if (!_lockProvider.Lock(requestHash))
                {
                    return new XResult<PersonalRegisterResponseV1>(null, ErrorCode.SUBMIT_REPEAT);
                }

                var existedRegisterInfo = _personalSubAccountRepository.QueryProvider.Where(x => x.AppId == request.AppId && x.UID == request.UserId).Count() > 0;
                if (existedRegisterInfo)
                {
                    return new XResult<PersonalRegisterResponseV1>(null, ErrorCode.INFO_EXISTED, new RequestException("开户信息已存在"));
                }

                var newId = IDGenerator.GenerateID();
                var newAccount = new PersonalSubAccount()
                {
                    Id = newId,
                    AppId = request.AppId,
                    UID = request.UserId,
                    IDCardNo = request.IDCardNo,
                    IDCardType = request.IDCardType,
                    RealName = request.RealName,
                    Mobile = request.Mobile,
                    Email = request.Email,
                    Status = PersonalInfoRegisterStatus.PROCESSING.ToString(),
                    UpdateTime = DateTime.Now
                };
                _personalSubAccountRepository.Add(newAccount);

                var saveResult = _personalSubAccountRepository.SaveChanges();
                if (!saveResult.Success)
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(_personalSubAccountRepository)}.SaveChanges()", "保存个人开户信息失败", saveResult.FirstException, request);
                    return new XResult<PersonalRegisterResponseV1>(null, ErrorCode.DB_UPDATE_FAILED, saveResult.FirstException);
                }

                String traceMethod = "Bill99UtilV1.Execute(/personalSeller/register)";

                _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.OK.ToString(), service, traceMethod, LogPhase.BEGIN, "开始调用快钱个人开户接口", request);

                var execResult = Bill99UtilV1.Execute<RawPersonalRegisterRequestV1, RawPersonalRegisterResponseV1>("/personalSeller/register", new RawPersonalRegisterRequestV1()
                {
                    requestId = IDGenerator.GenerateID().ToString().Substring(0, 10),
                    uId = request.UserId,
                    email = request.Email,
                    idCardNumber = request.IDCardNo,
                    idCardType = request.IDCardType,
                    //userFlag = "1",
                    mobile = request.Mobile,
                    platformCode = GlobalConfig.X99bill_COE_v1_PlatformCode,
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
                    return new XResult<PersonalRegisterResponseV1>(null, ErrorCode.DEPENDENT_API_CALL_FAILED, execResult.FirstException);
                }

                if (execResult.Value.ResponseCode == "0000")
                {
                    newAccount.Status = PersonalInfoRegisterStatus.SUCCESS.ToString();
                    newAccount.OpenId = execResult.Value.UserId;
                }
                else
                {
                    newAccount.Status = PersonalInfoRegisterStatus.FAILURE.ToString();
                }

                _personalSubAccountRepository.Update(newAccount);
                var updateResult = _personalSubAccountRepository.SaveChanges();
                if (!updateResult.Success)
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, traceMethod, "更新开户信息的OpenId失败", updateResult.FirstException, newAccount);
                    return new XResult<PersonalRegisterResponseV1>(null, ErrorCode.DB_UPDATE_FAILED, new DbUpdateException("更新开户信息失败"));
                }

                var resp = new PersonalRegisterResponseV1()
                {
                    Status = newAccount.Status,
                    UserId = newAccount.OpenId,
                    Msg = GetRegisterAuditStatusMsg(newAccount.Status)
                };

                return new XResult<PersonalRegisterResponseV1>(resp);
            }
            finally
            {
                _lockProvider.UnLock(requestHash);
            }
        }

        public XResult<QueryBankCardAcceptResponseV1> QueryBankCardAccept(QueryBankCardAcceptRequestV1 request)
        {
            if (request == null)
            {
                return new XResult<QueryBankCardAcceptResponseV1>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentNullException(nameof(request)));
            }

            String service = $"{this.GetType().FullName}.QueryBankCardAccept(...)";

            if (!request.IsValid)
            {
                _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(request)}.IsValid", LogPhase.ACTION, $"请求参数验证失败：{request.ErrorMessage}", request);
                return new XResult<QueryBankCardAcceptResponseV1>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentException(request.ErrorMessage));
            }

            String traceMethod = "Bill99UtilV1.Execute(/person/bankcard/accept)";

            _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.OK.ToString(), service, traceMethod, LogPhase.BEGIN, "开始调用快钱检测银行卡受理能力接口", request);

            var execResult = Bill99UtilV1.Execute<RawQueryBankCardAcceptRequestV1, RawQueryBankCardAcceptResponseV1>("/person/bankcard/accept", new RawQueryBankCardAcceptRequestV1()
            {
                requestId = IDGenerator.GenerateID().ToString().Substring(0, 10),
                uId = request.UserId,
                bankAcctId = request.BankCardNo,
                platformCode = GlobalConfig.X99bill_COE_v1_PlatformCode
            });

            _logger.Trace(TraceType.BLL.ToString(), (execResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), service, traceMethod, LogPhase.END, $"结束调用快钱检测银行卡受理能力接口", request);

            if (!execResult.Success || execResult.Value == null)
            {
                _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, traceMethod, "调用快钱检测银行卡受理能力接口失败", execResult.FirstException, execResult.Value);
                return new XResult<QueryBankCardAcceptResponseV1>(null, ErrorCode.DEPENDENT_API_CALL_FAILED, execResult.FirstException);
            }

            var respResult = execResult.Value;
            if (respResult.ResponseCode != "0000")
            {
                _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, traceMethod, LogPhase.ACTION, "检测银行卡受理能力返回结果", $"{respResult.ResponseCode}:{respResult.ResponseMessage}");
                return new XResult<QueryBankCardAcceptResponseV1>(null, ErrorCode.DEPENDENT_API_CALL_FAILED, new RemoteException(respResult.ResponseMessage));
            }

            var resp = new QueryBankCardAcceptResponseV1()
            {
                UserId = execResult.Value.UserId,
                BankCode = execResult.Value.bankId,
                BankName = execResult.Value.bankName,
                CardType = execResult.Value.cardType,
                OutTradeNo = execResult.Value.OutTradeNo,
                Status = CommonStatus.SUCCESS.ToString(),
                Msg = CommonStatus.SUCCESS.GetDescription()
            };

            return new XResult<QueryBankCardAcceptResponseV1>(resp);
        }

        public XResult<PersonalApplyBindCardResponseV1> ApplyBindCard(PersonalApplyBindCardRequestV1 request)
        {
            if (request == null)
            {
                return new XResult<PersonalApplyBindCardResponseV1>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentNullException(nameof(request)));
            }

            String service = $"{this.GetType().FullName}.ApplyBindCard(...)";

            if (!request.IsValid)
            {
                _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(request)}.IsValid", LogPhase.ACTION, $"请求参数验证失败：{request.ErrorMessage}", request);
                return new XResult<PersonalApplyBindCardResponseV1>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentException(request.ErrorMessage));
            }

            var requestHash = $"ApplyBindCard:{request.UserId}".GetHashCode();

            if (_lockProvider.Exists(requestHash))
            {
                return new XResult<PersonalApplyBindCardResponseV1>(null, ErrorCode.SUBMIT_REPEAT);
            }

            try
            {
                if (!_lockProvider.Lock(requestHash))
                {
                    return new XResult<PersonalApplyBindCardResponseV1>(null, ErrorCode.SUBMIT_REPEAT);
                }

                String traceMethod = $"{nameof(Bill99UtilV1)}.Execute(/person/bankcard/auth)";

                _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.OK.ToString(), service, traceMethod, LogPhase.BEGIN, "开始调用快钱绑卡鉴权接口", request);

                DateTime applyTime = DateTime.Now;

                var execResult = Bill99UtilV1.Execute<RawPersonalApplyBindCardRequestV1, RawPersonalApplyBindCardResponseV1>("/person/bankcard/auth", new RawPersonalApplyBindCardRequestV1()
                {
                    uId = request.UserId,
                    requestId = request.OutTradeNo,
                    platformCode = GlobalConfig.X99bill_COE_v1_PlatformCode,
                    idCardNumber = request.IDCardNo,
                    idCardType = request.IDCardType,
                    bankAcctId = request.BankCardNo,
                    bankName = request.BankName,
                    name = request.RealName,
                    mobile = request.Mobile
                });

                _logger.Trace(TraceType.BLL.ToString(), (execResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), service, traceMethod, LogPhase.END, $"结束调用快钱绑卡鉴权接口", request);

                if (!execResult.Success)
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, traceMethod, "绑卡鉴权失败", execResult.FirstException, request);
                    return new XResult<PersonalApplyBindCardResponseV1>(null, ErrorCode.DEPENDENT_API_CALL_FAILED, execResult.FirstException);
                }

                if (execResult.Value == null)
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, traceMethod, "快钱未返回任何数据");
                    return new XResult<PersonalApplyBindCardResponseV1>(null, ErrorCode.REMOTE_RETURN_NOTHING);
                }

                var resp = execResult.Value;
                if (resp.ResponseCode != "0000")
                {
                    _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, traceMethod, LogPhase.ACTION, $"{resp.ResponseCode}:{resp.ResponseMessage}");
                    return new XResult<PersonalApplyBindCardResponseV1>(null, ErrorCode.DEPENDENT_API_CALL_FAILED, new RemoteException(resp.ResponseMessage));
                }

                var respResult = new PersonalApplyBindCardResponseV1()
                {
                    OutTradeNo = resp.OutTradeNo,
                    UserId = resp.UserId,
                    ApplyToken = resp.token,
                    ApplyTime = applyTime,
                    Status = CommonStatus.SUCCESS.ToString(),
                    Msg = CommonStatus.SUCCESS.GetDescription()
                };

                return new XResult<PersonalApplyBindCardResponseV1>(respResult);
            }
            finally
            {
                _lockProvider.UnLock(requestHash);
            }
        }

        public XResult<PersonalWithdrawBindCardResponseV1> WithdrawBindCard(PersonalWithdrawBindCardRequestV1 request)
        {
            if (request == null)
            {
                return new XResult<PersonalWithdrawBindCardResponseV1>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentNullException(nameof(request)));
            }

            String service = $"{this.GetType().FullName}.WithdrawBindCard(...)";

            if (!request.IsValid)
            {
                _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(request)}.IsValid", LogPhase.ACTION, $"请求参数验证失败：{request.ErrorMessage}", request);
                return new XResult<PersonalWithdrawBindCardResponseV1>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentNullException(request.ErrorMessage));
            }

            var requestHash = $"bindcard:{request.UserId}".GetHashCode();

            if (_lockProvider.Exists(requestHash))
            {
                return new XResult<PersonalWithdrawBindCardResponseV1>(null, ErrorCode.SUBMIT_REPEAT);
            }

            try
            {
                if (!_lockProvider.Lock(requestHash))
                {
                    return new XResult<PersonalWithdrawBindCardResponseV1>(null, ErrorCode.SUBMIT_REPEAT);
                }

                var existedBindInfo = _withdrawBankCardBindInfoRepository.Exists(x => x.PayeeId == request.UserId && x.BankCardNo == request.BankCardNo);
                if (existedBindInfo)
                {
                    return new XResult<PersonalWithdrawBindCardResponseV1>(null, ErrorCode.INFO_EXISTED, new ArgumentException("绑卡信息已存在"));
                }

                var newBindInfo = new WithdrawBankCardBindInfo()
                {
                    Id = IDGenerator.GenerateID(),
                    AppId = request.AppId,
                    PayeeId = request.UserId,
                    BankCardNo = request.BankCardNo,
                    Mobile = request.Mobile,
                    ApplyTime = DateTime.Now,
                    BindStatus = WithdrawBindCardStatus.PROCESSING.ToString()
                };

                using (var tx = new TransactionScope())
                {
                    _withdrawBankCardBindInfoRepository.Add(newBindInfo);
                    var saveResult = _withdrawBankCardBindInfoRepository.SaveChanges();
                    if (!saveResult.Success)
                    {
                        _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(_withdrawBankCardBindInfoRepository)}.SaveChanges()", "保存提现绑卡信息失败", saveResult.FirstException, request);
                        return new XResult<PersonalWithdrawBindCardResponseV1>(null, ErrorCode.DB_UPDATE_FAILED, saveResult.FirstException);
                    }

                    String traceMethod = "Bill99UtilV1.Execute(/person/bankcard/bind)";

                    _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.OK.ToString(), service, traceMethod, LogPhase.BEGIN);

                    var execResult = Bill99UtilV1.Execute<RawPersonalWithdrawBindCardRequestV1, RawPersonalWithdrawBindCardResponseV1>("/person/bankcard/bind", new RawPersonalWithdrawBindCardRequestV1()
                    {
                        requestId = request.OutTradeNo,
                        uId = request.UserId,
                        platformCode = GlobalConfig.X99bill_COE_v1_PlatformCode,
                        idCardNumber = request.IDCardNo,
                        idCardType = request.IDCardType,
                        name = request.RealName,
                        token = request.ApplyToken,
                        validCode = request.SmsValidCode,
                        bankName = request.BankName,
                        bankAcctId = request.BankCardNo,
                        mobile = request.Mobile
                    });

                    _logger.Trace(TraceType.BLL.ToString(), (execResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), service, traceMethod, LogPhase.END);

                    if (!execResult.Success)
                    {
                        _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, traceMethod, "提现绑卡失败", execResult.FirstException, request);
                        return new XResult<PersonalWithdrawBindCardResponseV1>(null, ErrorCode.DEPENDENT_API_CALL_FAILED, execResult.FirstException);
                    }

                    if (execResult.Value == null)
                    {
                        _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.OK.ToString(), service, traceMethod, LogPhase.ACTION, "快钱未返回任何数据");
                        return new XResult<PersonalWithdrawBindCardResponseV1>(null, ErrorCode.REMOTE_RETURN_NOTHING);
                    }

                    if (execResult.Value.ResponseCode != "0000")
                    {
                        _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, traceMethod, "提现绑卡失败", null, execResult.Value);
                        return new XResult<PersonalWithdrawBindCardResponseV1>(null, ErrorCode.DEPENDENT_API_CALL_FAILED, new RemoteException(execResult.Value.ResponseMessage));
                    }

                    newBindInfo.BindStatus = WithdrawBindCardStatus.SUCCESS.ToString();
                    var updateResult = _withdrawBankCardBindInfoRepository.SaveChanges();
                    if (!updateResult.Success)
                    {
                        _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, traceMethod, "更新绑卡信息失败", updateResult.FirstException, newBindInfo);
                        return new XResult<PersonalWithdrawBindCardResponseV1>(null, ErrorCode.DB_UPDATE_FAILED, new RequestException("更新绑卡信息失败"));
                    }

                    tx.Complete();

                    var respResult = new PersonalWithdrawBindCardResponseV1()
                    {
                        UserId = execResult.Value.UserId,
                        OutTradeNo = execResult.Value.OutTradeNo,
                        Status = CommonStatus.SUCCESS.ToString(),
                        Msg = $"绑卡{CommonStatus.SUCCESS.GetDescription()}"
                    };

                    return new XResult<PersonalWithdrawBindCardResponseV1>(respResult);
                }
            }
            finally
            {
                _lockProvider.UnLock(requestHash);
            }
        }

        public XResult<PersonalBindCardSendSmsValidCodeResponseV1> MobileCheck(PersonalBindCardSendSmsValidCodeRequestV1 request)
        {
            if (request == null)
            {
                return new XResult<PersonalBindCardSendSmsValidCodeResponseV1>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentNullException(nameof(request)));
            }

            String service = $"{this.GetType().FullName}.MobileCheck(...)";

            if (!request.IsValid)
            {
                _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(request)}.IsValid", LogPhase.ACTION, $"请求参数验证失败：{request.ErrorMessage}", request);
                return new XResult<PersonalBindCardSendSmsValidCodeResponseV1>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentNullException(request.ErrorMessage));
            }

            var requestHash = $"MobileCheck:{request.UserId}".GetHashCode();

            if (_lockProvider.Exists(requestHash))
            {
                return new XResult<PersonalBindCardSendSmsValidCodeResponseV1>(null, ErrorCode.SUBMIT_REPEAT);
            }

            try
            {
                if (!_lockProvider.Lock(requestHash))
                {
                    return new XResult<PersonalBindCardSendSmsValidCodeResponseV1>(null, ErrorCode.SUBMIT_REPEAT);
                }

                String traceMethod = "Bill99UtilV1.Execute(/personalSeller/mobileCheck)";

                _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.OK.ToString(), service, traceMethod, LogPhase.BEGIN);

                var execResult = Bill99UtilV1.Execute<RawPersonalBindCardSendSmsValidCodeRequestV1, RawPersonalBindCardSendSmsValidCodeResponseV1>("/personalSeller/mobileCheck", new RawPersonalBindCardSendSmsValidCodeRequestV1()
                {
                    identitycardId = request.IDCardNo,
                    name = request.RealName,
                    phonNumber = request.Mobile,
                    platformCode = GlobalConfig.X99bill_COE_v1_PlatformCode,
                    requestId = request.OutTradeNo,
                    requestTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    uId = request.UserId
                });

                _logger.Trace(TraceType.BLL.ToString(), (execResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), service, traceMethod, LogPhase.END);

                if (!execResult.Success)
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, traceMethod, "手机号验证失败", execResult.FirstException, request);
                    return new XResult<PersonalBindCardSendSmsValidCodeResponseV1>(null, ErrorCode.FAILURE, execResult.FirstException);
                }

                if (execResult.Value == null)
                {
                    _logger.Trace(TraceType.BLL.ToString(), (execResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), service, traceMethod, LogPhase.ACTION, "快钱未返回任何数据");
                    return new XResult<PersonalBindCardSendSmsValidCodeResponseV1>(null, ErrorCode.REMOTE_RETURN_NOTHING);
                }

                if (execResult.Value.ResponseCode != "0000")
                {
                    return new XResult<PersonalBindCardSendSmsValidCodeResponseV1>(null, ErrorCode.FAILURE);
                }

                return new XResult<PersonalBindCardSendSmsValidCodeResponseV1>(new PersonalBindCardSendSmsValidCodeResponseV1()
                {
                    UserId = execResult.Value.UserId,
                    OutTradeNo = execResult.Value.OutTradeNo,
                    Status = CommonStatus.SUCCESS.ToString(),
                    Msg = CommonStatus.SUCCESS.GetDescription()
                });
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
                case "PROCESSING":
                    return PersonalInfoRegisterStatus.PROCESSING.GetDescription();
                case "FAILURE":
                    return PersonalInfoRegisterStatus.FAILURE.GetDescription();
            }

            return String.Empty;
        }
    }
}
