using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
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
using Lotus.Core.Collections;
using Lotus.Logging;

namespace CPI.Services.SettleServices
{
    public class PersonalServiceV1 : IPersonalServiceV1
    {
        private static readonly LockProvider _lockProvider = new LockProvider();
        private static readonly ILogger _logger = LogManager.GetLogger();

        private readonly IPersonalSubAccountRepository _personalSubAccountRepository = null;
        private readonly IWithdrawBankCardBindInfoRepository _withdrawBankCardBindInfoRepository = null;
        private readonly IAllotAmountWithdrawOrderRepository _withdrawOrderRepository = null;

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
                    userFlag = "1",
                    mobile = request.Mobile,
                    platformCode = GlobalConfig.X99bill_COE_v1_PlatformCode,
                    name = request.RealName
                });

                _logger.Trace(TraceType.BLL.ToString(), (execResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), service, traceMethod, LogPhase.END, $"结束调用快钱个人开户接口", request);

                Boolean needRollback = false;

                if (!execResult.Success || execResult.Value == null)
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, traceMethod, "个人开户失败", execResult.FirstException, execResult.Value);
                    needRollback = true;
                }

                if (execResult.Value != null && execResult.Value.ResponseCode != "0000")
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, traceMethod, $"{execResult.Value.ResponseCode}:{execResult.Value.ResponseMessage}", null, execResult.Value);
                    needRollback = true;
                }

                if (needRollback)
                {
                    _personalSubAccountRepository.Remove(newAccount);
                    saveResult = _personalSubAccountRepository.SaveChanges();
                    if (!saveResult.Success)
                    {
                        _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, traceMethod, "删除个人开户记录失败", saveResult.FirstException, newAccount);
                    }

                    _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.OK.ToString(), service, traceMethod, LogPhase.ACTION, "已删除开户失败的记录");
                    return new XResult<PersonalRegisterResponseV1>(null, ErrorCode.DEPENDENT_API_CALL_FAILED, execResult.Value != null ? new RemoteException(execResult.Value.ResponseMessage) : execResult.FirstException);
                }

                newAccount.Status = PersonalInfoRegisterStatus.SUCCESS.ToString();
                newAccount.OpenId = execResult.Value.UserId;
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

        public XResult<PersonalRegisterInfoQueryResponseV1> QueryPersonalInfo(PersonalRegisterInfoQueryRequestV1 request)
        {
            if (request == null)
            {
                return new XResult<PersonalRegisterInfoQueryResponseV1>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentNullException(nameof(request)));
            }

            if (!request.IsValid)
            {
                return new XResult<PersonalRegisterInfoQueryResponseV1>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentException(request.ErrorMessage));
            }

            try
            {
                var userInfo = (from t0 in _personalSubAccountRepository.QueryProvider
                                where t0.UID == request.UserId
                                select t0).FirstOrDefault();

                if (userInfo == null)
                {
                    return new XResult<PersonalRegisterInfoQueryResponseV1>(null, ErrorCode.INFO_NOT_EXIST);
                }

                var bindcardInfo = (from t0 in _withdrawBankCardBindInfoRepository.QueryProvider
                                    where t0.PayeeId == request.UserId
                                    select t0).FirstOrDefault();

                var resp = new PersonalRegisterInfoQueryResponseV1()
                {
                    UserId = userInfo.UID,
                    IDCardNo = userInfo.IDCardNo,
                    IDCardType = userInfo.IDCardType,
                    RealName = userInfo.RealName,
                    Mobile = bindcardInfo != null ? bindcardInfo.Mobile : userInfo.Mobile,
                    BankCardNo = bindcardInfo != null ? bindcardInfo.BankCardNo : null,
                    BankName = bindcardInfo != null ? bindcardInfo.BankName : null,
                    Email = userInfo.Email,
                    Status = userInfo.Status
                };

                if (resp.BankCardNo.IsNullOrWhiteSpace())
                {
                    resp.BankCardNo = "(未绑卡)";
                }

                if (resp.BankName.IsNullOrWhiteSpace())
                {
                    resp.BankName = "(未知)";
                }

                return new XResult<PersonalRegisterInfoQueryResponseV1>(resp);
            }
            catch (Exception ex)
            {
                return new XResult<PersonalRegisterInfoQueryResponseV1>(null, ErrorCode.DB_QUERY_FAILED, ex);
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
                    requestId = IDGenerator.GenerateID().ToString().Substring(0, 10),
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

                var existedBindInfo = _withdrawBankCardBindInfoRepository.Exists(x => x.PayeeId == request.UserId);
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

                    String traceMethod = $"{nameof(Bill99UtilV1)}.Execute(/person/bankcard/bind)";

                    _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.OK.ToString(), service, traceMethod, LogPhase.BEGIN);

                    var execResult = Bill99UtilV1.Execute<RawPersonalWithdrawBindCardRequestV1, RawPersonalWithdrawBindCardResponseV1>("/person/bankcard/bind", new RawPersonalWithdrawBindCardRequestV1()
                    {
                        requestId = IDGenerator.GenerateID().ToString().Substring(0, 10),
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

                    newBindInfo.BankName = request.BankName;
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
                    requestId = IDGenerator.GenerateID().ToString().Substring(0, 10),
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
                    Status = CommonStatus.SUCCESS.ToString(),
                    Msg = CommonStatus.SUCCESS.GetDescription()
                });
            }
            finally
            {
                _lockProvider.UnLock(requestHash);
            }
        }

        public XResult<PersonalRegisterContractInfoQueryResponseV1> QueryContract(PersonalRegisterContractInfoQueryRequestV1 request)
        {
            if (request == null)
            {
                return new XResult<PersonalRegisterContractInfoQueryResponseV1>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentNullException(nameof(request)));
            }

            String service = $"{this.GetType().FullName}.QueryContract(...)";

            if (!request.IsValid)
            {
                _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(request)}.IsValid", LogPhase.ACTION, $"请求参数验证失败：{request.ErrorMessage}", request);
                return new XResult<PersonalRegisterContractInfoQueryResponseV1>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentException(request.ErrorMessage));
            }

            String traceMethod = "Bill99UtilV1.Execute(/econtractQuery)";

            _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.OK.ToString(), service, traceMethod, LogPhase.BEGIN, "开始调用合同查询接口", request);

            var execResult = Bill99UtilV1.Execute<RawPersonalRegisterContractInfoQueryRequestV1, RawPersonalRegisterContractInfoQueryResponseV1>("/econtractQuery", new RawPersonalRegisterContractInfoQueryRequestV1()
            {
                requestId = IDGenerator.GenerateID().ToString().Substring(0, 10),
                applyId = request.UserId,
                platformCode = GlobalConfig.X99bill_COE_v1_PlatformCode
            });

            _logger.Trace(TraceType.BLL.ToString(), (execResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), service, traceMethod, LogPhase.END, $"结束调用合同查询接口", request);

            if (!execResult.Success || execResult.Value == null)
            {
                _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, traceMethod, "调用合同查询接口失败", execResult.FirstException, execResult.Value);
                return new XResult<PersonalRegisterContractInfoQueryResponseV1>(null, ErrorCode.DEPENDENT_API_CALL_FAILED, execResult.FirstException);
            }

            var respResult = execResult.Value;
            if (respResult.code != "0000")
            {
                _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, traceMethod, LogPhase.ACTION, "合同查询返回结果", $"{respResult.code}:{respResult.errorMsg}");
                return new XResult<PersonalRegisterContractInfoQueryResponseV1>(null, ErrorCode.DEPENDENT_API_CALL_FAILED, new RemoteException(respResult.errorMsg));
            }

            var resp = new PersonalRegisterContractInfoQueryResponseV1()
            {
                UserId = execResult.Value.applyId,
                ContractNo = execResult.Value.contractNum,
                FileId = execResult.Value.fssId,
                SignDate = execResult.Value.submitDate
            };

            switch (respResult.signStatus)
            {
                case "0":
                    resp.Status = PersonalRegisterContractSignStatus.WAIT_FOR_SIGN.ToString();
                    resp.Msg = PersonalRegisterContractSignStatus.WAIT_FOR_SIGN.GetDescription();
                    break;
                case "1":
                    resp.Status = PersonalRegisterContractSignStatus.SUCCESS.ToString();
                    resp.Msg = PersonalRegisterContractSignStatus.SUCCESS.GetDescription();
                    break;
                case "2":
                    resp.Status = PersonalRegisterContractSignStatus.FAILURE.ToString();
                    resp.Msg = PersonalRegisterContractSignStatus.FAILURE.GetDescription();
                    break;
                case "3":
                    resp.Status = PersonalRegisterContractSignStatus.WAIT_FOR_ACTIVE.ToString();
                    resp.Msg = PersonalRegisterContractSignStatus.WAIT_FOR_ACTIVE.GetDescription();
                    break;
            }

            return new XResult<PersonalRegisterContractInfoQueryResponseV1>(resp);
        }

        public XResult<PersonalRegisterContractSignResponseV1> SignContract(PersonalRegisterContractSignRequestV1 request)
        {
            if (request == null)
            {
                return new XResult<PersonalRegisterContractSignResponseV1>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentNullException(nameof(request)));
            }

            String service = $"{this.GetType().FullName}.SignContract(...)";

            if (!request.IsValid)
            {
                _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(request)}.IsValid", LogPhase.ACTION, $"请求参数验证失败：{request.ErrorMessage}", request);
                return new XResult<PersonalRegisterContractSignResponseV1>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentException(request.ErrorMessage));
            }

            var requestHash = $"SignContract:{request.UserId}".GetHashCode();

            if (_lockProvider.Exists(requestHash))
            {
                return new XResult<PersonalRegisterContractSignResponseV1>(null, ErrorCode.SUBMIT_REPEAT);
            }

            try
            {
                if (!_lockProvider.Lock(requestHash))
                {
                    return new XResult<PersonalRegisterContractSignResponseV1>(null, ErrorCode.SUBMIT_REPEAT);
                }

                //先判断是否开过户
                var accountRegistered = _personalSubAccountRepository.QueryProvider.Count(x => x.UID == request.UserId) > 0;
                if (!accountRegistered)
                {
                    return new XResult<PersonalRegisterContractSignResponseV1>(null, ErrorCode.UN_REGISTERED);
                }

                String traceMethod = $"{nameof(Bill99UtilV1)}.Execute(/signContract)";

                _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.OK.ToString(), service, traceMethod, LogPhase.BEGIN, "开始调用快钱合同签约接口", request);

                DateTime applyTime = DateTime.Now;

                var execResult = Bill99UtilV1.Execute<RawPersonalRegisterContractSignRequestV1, RawPersonalRegisterContractSignResponseV1>("/signContract", new RawPersonalRegisterContractSignRequestV1()
                {
                    applyId = request.UserId,
                    requestId = IDGenerator.GenerateID().ToString().Substring(0, 10),
                    platformCode = GlobalConfig.X99bill_COE_v1_PlatformCode,
                    signType = "1"
                });

                _logger.Trace(TraceType.BLL.ToString(), (execResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), service, traceMethod, LogPhase.END, $"结束调用快钱合同签约接口", request);

                if (!execResult.Success)
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, traceMethod, "合同签约失败", execResult.FirstException, request);
                    return new XResult<PersonalRegisterContractSignResponseV1>(null, ErrorCode.DEPENDENT_API_CALL_FAILED, execResult.FirstException);
                }

                if (execResult.Value == null)
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, traceMethod, "快钱未返回任何数据");
                    return new XResult<PersonalRegisterContractSignResponseV1>(null, ErrorCode.REMOTE_RETURN_NOTHING);
                }

                var resp = execResult.Value;
                if (resp.code != "0000")
                {
                    _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, traceMethod, LogPhase.ACTION, $"{resp.code}:{resp.errorMsg}");
                    return new XResult<PersonalRegisterContractSignResponseV1>(null, ErrorCode.DEPENDENT_API_CALL_FAILED, new RemoteException(resp.errorMsg));
                }

                var respResult = new PersonalRegisterContractSignResponseV1()
                {
                    UserId = resp.applyId,
                    StartDate = resp.startDate,
                    EndDate = resp.endDate,
                    SignDate = resp.submitDate
                };

                switch (resp.signStatus)
                {
                    case "-1":
                        respResult.Status = PersonalRegisterContractSignStatus.FAILURE.ToString();
                        respResult.Msg = PersonalRegisterContractSignStatus.CONTRACT_NOT_EXIST.GetDescription();
                        break;
                    case "0":
                        respResult.Status = PersonalRegisterContractSignStatus.WAIT_FOR_SIGN.ToString();
                        respResult.Msg = PersonalRegisterContractSignStatus.WAIT_FOR_SIGN.GetDescription();
                        break;
                    case "1":
                        respResult.Status = PersonalRegisterContractSignStatus.SUCCESS.ToString();
                        respResult.Msg = PersonalRegisterContractSignStatus.SUCCESS.GetDescription();
                        break;
                    case "2":
                        respResult.Status = PersonalRegisterContractSignStatus.FAILURE.ToString();
                        respResult.Msg = PersonalRegisterContractSignStatus.SIGN_REFUSE.GetDescription();
                        break;
                    case "3":
                        respResult.Status = PersonalRegisterContractSignStatus.FAILURE.ToString();
                        respResult.Msg = PersonalRegisterContractSignStatus.WAIT_FOR_ACTIVE.GetDescription();
                        break;
                    case "4":
                        respResult.Status = PersonalRegisterContractSignStatus.WAIT_FOR_CONVERT_TO_PDF.ToString();
                        respResult.Msg = PersonalRegisterContractSignStatus.WAIT_FOR_CONVERT_TO_PDF.GetDescription();
                        break;
                    case "6":
                        respResult.Status = PersonalRegisterContractSignStatus.SIGNING.ToString();
                        respResult.Msg = PersonalRegisterContractSignStatus.SIGNING.GetDescription();
                        break;
                    case "7":
                        respResult.Status = PersonalRegisterContractSignStatus.FAILURE.ToString();
                        respResult.Msg = PersonalRegisterContractSignStatus.FAILURE.GetDescription();
                        break;
                }

                return new XResult<PersonalRegisterContractSignResponseV1>(respResult);
            }
            finally
            {
                _lockProvider.UnLock(requestHash);
            }
        }

        public XResult<PersonalWithdrawResponseV1> ApplyWithdraw(PersonalWithdrawRequestV1 request)
        {
            if (request == null)
            {
                return new XResult<PersonalWithdrawResponseV1>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentNullException(nameof(request)));
            }

            String service = $"{this.GetType().FullName}.ApplyWithdraw(...)";

            if (!request.IsValid)
            {
                _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(request)}.IsValid", LogPhase.ACTION, $"请求参数验证失败：{request.ErrorMessage}", request);
                return new XResult<PersonalWithdrawResponseV1>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentException(request.ErrorMessage));
            }

            var requestHash = $"ApplyWithdraw:{request.PayeeId}".GetHashCode();

            if (_lockProvider.Exists(requestHash))
            {
                return new XResult<PersonalWithdrawResponseV1>(null, ErrorCode.SUBMIT_REPEAT);
            }

            try
            {
                if (!_lockProvider.Lock(requestHash))
                {
                    return new XResult<PersonalWithdrawResponseV1>(null, ErrorCode.SUBMIT_REPEAT);
                }

                var withdrawOrderExisted = (from t0 in _withdrawOrderRepository.QueryProvider
                                            where t0.OutTradeNo == request.OutTradeNo
                                            select t0).Count() > 0;

                if (withdrawOrderExisted)
                {
                    return new XResult<PersonalWithdrawResponseV1>(null, ErrorCode.OUT_TRADE_NO_EXISTED);
                }

                var userAccountInfo = _personalSubAccountRepository.QueryProvider.FirstOrDefault(x => x.UID == request.PayeeId);
                if (userAccountInfo == null)
                {
                    return new XResult<PersonalWithdrawResponseV1>(null, ErrorCode.UN_REGISTERED);
                }

                var bindcardInfo = _withdrawBankCardBindInfoRepository.QueryProvider.FirstOrDefault(x => x.PayeeId == request.PayeeId);
                if (bindcardInfo == null)
                {
                    return new XResult<PersonalWithdrawResponseV1>(null, ErrorCode.NO_BANKCARD_BOUND);
                }

                var newId = IDGenerator.GenerateID();
                var newOrder = new AllotAmountWithdrawOrder()
                {
                    Id = newId,
                    AppId = request.AppId,
                    TradeNo = newId.ToString(),
                    PayeeId = request.PayeeId,
                    Amount = request.Amount,
                    IsPlatformMerchant = request.IsPlatformMerchant,
                    CustomerFee = GetCustomerWithdrawFee(request.Amount),
                    MerchantFee = 0,
                    OutTradeNo = request.OutTradeNo,
                    ApplyTime = DateTime.Now,
                    Status = WithdrawOrderStatus.APPLY.ToString(),
                    SettlePeriod = request.SettlePeriod,
                    OrderType = request.OrderType,
                    PayMode = request.PayMode,
                    Remark = request.Remark
                };

                RawPersonalWithdrawResponseV1 resp = null;

                using (var tx = new TransactionScope())
                {
                    _withdrawOrderRepository.Add(newOrder);
                    var saveResult = _withdrawOrderRepository.SaveChanges();

                    if (!saveResult.Success)
                    {
                        _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(_withdrawOrderRepository)}.SaveChanges()", "创建提现单失败", saveResult.FirstException, newOrder);
                        return new XResult<PersonalWithdrawResponseV1>(null, ErrorCode.DB_UPDATE_FAILED, saveResult.FirstException);
                    }

                    String traceMethod = $"{nameof(Bill99UtilHAT)}.Execute(/account/merchantWithdraw)";

                    _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.OK.ToString(), service, traceMethod, LogPhase.BEGIN, "开始调用快钱HAT提现接口", request);

                    DateTime applyTime = DateTime.Now;

                    var execResult = Bill99UtilHAT.Execute<RawPersonalWithdrawRequestV1, RawPersonalWithdrawResponseV1>("/account/merchantWithdraw", new RawPersonalWithdrawRequestV1()
                    {
                        functionCode = request.FunctionCode,
                        outTradeNo = request.OutTradeNo,
                        merchantName = request.MerchantName,
                        merchantUId = request.PayeeId,
                        isPlatformMerchant = request.IsPlatformMerchant,
                        bankAcctName = userAccountInfo.RealName,
                        amount = request.Amount,
                        bankAcctId = bindcardInfo.BankCardNo,
                        bankName = bindcardInfo.BankName,
                        payMode = request.PayMode,
                        orderType = request.OrderType
                    });

                    _logger.Trace(TraceType.BLL.ToString(), (execResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), service, traceMethod, LogPhase.END, $"结束调用快钱HAT提现接口", request);

                    if (!execResult.Success)
                    {
                        _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, traceMethod, "提现失败", execResult.FirstException, request);
                        return new XResult<PersonalWithdrawResponseV1>(null, ErrorCode.DEPENDENT_API_CALL_FAILED, execResult.FirstException);
                    }

                    if (execResult.Value == null)
                    {
                        _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, traceMethod, "快钱未返回任何数据");
                        return new XResult<PersonalWithdrawResponseV1>(null, ErrorCode.REMOTE_RETURN_NOTHING);
                    }

                    resp = execResult.Value;
                    if (resp.ResponseCode != "0000")
                    {
                        _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, traceMethod, LogPhase.ACTION, $"{resp.ResponseCode}:{resp.ResponseMessage}");
                        return new XResult<PersonalWithdrawResponseV1>(null, ErrorCode.DEPENDENT_API_CALL_FAILED, new RemoteException(resp.ResponseMessage));
                    }

                    tx.Complete();
                }

                newOrder.Status = WithdrawBindCardStatus.PROCESSING.ToString();
                var updateResult = _withdrawOrderRepository.SaveChanges();

                if (!updateResult.Success)
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(_withdrawOrderRepository)}.SaveChanges()", "更新提现单状态失败", updateResult.FirstException, newOrder);
                }

                var respResult = new PersonalWithdrawResponseV1()
                {
                    OutTradeNo = resp.outTradeNo,
                    Status = CommonStatus.SUCCESS.ToString(),
                    Msg = $"申请{CommonStatus.SUCCESS.GetDescription()}"
                };

                return new XResult<PersonalWithdrawResponseV1>(respResult);
            }
            finally
            {
                _lockProvider.UnLock(requestHash);
            }
        }

        public XResult<PersonalAccountBalanceQueryResponseV1> QueryAccountBalance(PersonalAccountBalanceQueryRequestV1 request)
        {
            if (request == null)
            {
                return new XResult<PersonalAccountBalanceQueryResponseV1>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentNullException(nameof(request)));
            }

            String service = $"{this.GetType().FullName}.QueryAccountBalance(...)";

            if (!request.IsValid)
            {
                _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(request)}.IsValid", LogPhase.ACTION, $"请求参数验证失败：{request.ErrorMessage}", request);
                return new XResult<PersonalAccountBalanceQueryResponseV1>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentException(request.ErrorMessage));
            }

            String traceMethod = $"{nameof(Bill99UtilHAT)}.Execute(/account/balance)";

            _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.OK.ToString(), service, traceMethod, LogPhase.BEGIN, "开始调用余额查询接口", request);

            var execResult = Bill99UtilHAT.Execute<RawPersonalAccountBalanceQueryRequestV1, RawPersonalAccountBalanceQueryResponseV1>("/account/balance", new RawPersonalAccountBalanceQueryRequestV1()
            {
                uId = request.UserId,
                isPlatform = request.IsPlatform,
                accountType = request.AccountType.Split(new Char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
            });

            _logger.Trace(TraceType.BLL.ToString(), (execResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), service, traceMethod, LogPhase.END, $"结束调用余额查询接口", request);

            if (!execResult.Success || execResult.Value == null)
            {
                _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, traceMethod, "调用余额查询接口失败", execResult.FirstException, execResult.Value);
                return new XResult<PersonalAccountBalanceQueryResponseV1>(null, ErrorCode.DEPENDENT_API_CALL_FAILED, execResult.FirstException);
            }

            var respResult = execResult.Value;
            if (respResult.ResponseCode != "0000")
            {
                _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, traceMethod, LogPhase.ACTION, "余额查询返回结果", $"{respResult.ResponseCode}:{respResult.ResponseMessage}");
                return new XResult<PersonalAccountBalanceQueryResponseV1>(null, ErrorCode.DEPENDENT_API_CALL_FAILED, new RemoteException(respResult.ResponseMessage));
            }

            var accountBalanceList = respResult.accountBalanceList;
            if (accountBalanceList == null || accountBalanceList.Count() == 0)
            {
                return new XResult<PersonalAccountBalanceQueryResponseV1>(null, ErrorCode.INFO_NOT_EXIST, new ArgumentException("账户余额信息不存在"));
            }

            var balanceInfo = accountBalanceList.First();

            var resp = new PersonalAccountBalanceQueryResponseV1()
            {
                AccountName = balanceInfo.accountName,
                Balance = balanceInfo.balance,
                Status = CommonStatus.SUCCESS.ToString(),
                Msg = CommonStatus.SUCCESS.GetDescription()
            };

            return new XResult<PersonalAccountBalanceQueryResponseV1>(resp);
        }

        public XResult<WithdrawOrderQueryResponseV1> QueryWithdrawOrder(WithdrawOrderQueryRequestV1 request)
        {
            if (request == null)
            {
                return new XResult<WithdrawOrderQueryResponseV1>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentNullException(nameof(request)));
            }

            String service = $"{this.GetType().FullName}.QueryWithdrawOrder(...)";

            if (!request.IsValid)
            {
                return new XResult<WithdrawOrderQueryResponseV1>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentException(request.ErrorMessage));
            }

            var requestHash = $"QueryWithdrawOrder:{request.OutTradeNo}".GetHashCode();

            if (_lockProvider.Exists(requestHash))
            {
                return new XResult<WithdrawOrderQueryResponseV1>(null, ErrorCode.SUBMIT_REPEAT);
            }

            try
            {
                var withdrawOrder = _withdrawOrderRepository.QueryProvider.FirstOrDefault(x => x.OutTradeNo == request.OutTradeNo);
                if (withdrawOrder == null)
                {
                    return new XResult<WithdrawOrderQueryResponseV1>(null, ErrorCode.INFO_NOT_EXIST, new ArgumentException("提现单不存在"));
                }

                if (withdrawOrder.Status == WithdrawOrderStatus.SUCCESS.ToString()
                    || withdrawOrder.Status == WithdrawOrderStatus.FAILURE.ToString()
                    || String.Compare(request.QueryMode, "QUERY", true) == 0)
                {
                    return new XResult<WithdrawOrderQueryResponseV1>(new WithdrawOrderQueryResponseV1()
                    {
                        PayeeId = withdrawOrder.PayeeId,
                        OrderAmount = withdrawOrder.Amount,
                        OutTradeNo = withdrawOrder.OutTradeNo,
                        IsPlatformPayee = withdrawOrder.IsPlatformPayee,
                        OrderType = withdrawOrder.OrderType,
                        PayMode = withdrawOrder.PayMode,
                        Remark = withdrawOrder.Remark,
                        TradeBeginTime = withdrawOrder.ApplyTime,
                        TradeEndTime = withdrawOrder.CompleteTime,
                        Status = withdrawOrder.Status,
                        Msg = GetWithdrawOrderStatusDescription(withdrawOrder.Status)
                    });
                }

                if (!_lockProvider.Lock(requestHash))
                {
                    return new XResult<WithdrawOrderQueryResponseV1>(null, ErrorCode.SUBMIT_REPEAT);
                }

                String traceMethod = $"{nameof(Bill99UtilHAT)}.Execute(/order/detail)";

                _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.OK.ToString(), service, traceMethod, LogPhase.BEGIN, "开始调用快钱HAT查询提现订单详情接口", request);

                var queryResult = Bill99UtilHAT.Execute<RawWithdrawOrderQueryRequestV1, RawWithdrawOrderQueryResponseV1>("/order/detail", new RawWithdrawOrderQueryRequestV1()
                {
                    outTradeNo = request.OutTradeNo
                });

                _logger.Trace(TraceType.BLL.ToString(), (queryResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), service, traceMethod, LogPhase.END, $"结束调用快钱HAT查询提现订单详情接口", queryResult.Value);

                if (!queryResult.Success)
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, traceMethod, "查询提现订单详情失败", queryResult.FirstException, new Object[] { request, queryResult.Value });
                    return new XResult<WithdrawOrderQueryResponseV1>(null, queryResult.ErrorCode, queryResult.FirstException);
                }

                if (queryResult.Value == null)
                {
                    return new XResult<WithdrawOrderQueryResponseV1>(null, ErrorCode.REMOTE_RETURN_NOTHING, new RemoteException("快钱未返回任何数据"));
                }

                var respResult = queryResult.Value;

                if (respResult.ResponseCode != "0000")
                {
                    _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, traceMethod, LogPhase.ACTION, $"{queryResult.Value.ResponseCode}:{queryResult.Value.ResponseMessage}");
                    return new XResult<WithdrawOrderQueryResponseV1>(null, ErrorCode.FAILURE, new RemoteException($"{queryResult.Value.ResponseCode}:{queryResult.Value.ResponseMessage}"));
                }

                switch (respResult.orderStatus)
                {
                    case "0":
                        withdrawOrder.Status = WithdrawOrderStatus.APPLY.ToString();
                        break;
                    case "3":
                    case "8":
                        withdrawOrder.Status = WithdrawOrderStatus.PROCESSING.ToString();
                        break;
                    case "1":
                        withdrawOrder.Status = WithdrawOrderStatus.SUCCESS.ToString();
                        withdrawOrder.CompleteTime = 解析傻逼快钱返回的时间(respResult.txnEndTime);
                        break;
                    default:
                        withdrawOrder.Status = WithdrawOrderStatus.FAILURE.ToString();
                        withdrawOrder.CompleteTime = 解析傻逼快钱返回的时间(respResult.txnEndTime);
                        break;
                }

                if (respResult.isPlatformPayee.HasValue())
                {
                    withdrawOrder.IsPlatformPayee = respResult.isPlatformPayee;
                }

                _withdrawOrderRepository.Update(withdrawOrder);
                var saveResult = _withdrawOrderRepository.SaveChanges();
                if (!saveResult.Success)
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(_withdrawOrderRepository)}.SaveChanges()", "无法更新提现单状态", saveResult.FirstException, withdrawOrder);
                }

                return new XResult<WithdrawOrderQueryResponseV1>(new WithdrawOrderQueryResponseV1()
                {
                    PayeeId = withdrawOrder.PayeeId,
                    OrderAmount = withdrawOrder.Amount,
                    OutTradeNo = withdrawOrder.OutTradeNo,
                    IsPlatformPayee = withdrawOrder.IsPlatformPayee,
                    OrderType = withdrawOrder.OrderType,
                    PayMode = withdrawOrder.PayMode,
                    Remark = withdrawOrder.Remark,
                    TradeBeginTime = withdrawOrder.ApplyTime,
                    TradeEndTime = withdrawOrder.CompleteTime,
                    Status = withdrawOrder.Status,
                    Msg = GetWithdrawOrderStatusDescription(withdrawOrder.Status)
                });
            }
            finally
            {
                _lockProvider.UnLock(requestHash);
            }
        }

        private DateTime 解析傻逼快钱返回的时间(String endTime)
        {
            //20190220141838
            //2019-02-20 14:18:38
            if (Regex.IsMatch(endTime, @"\d{14}", RegexOptions.IgnoreCase))
            {
                var chars = new Char[] { endTime[0], endTime[1], endTime[2], endTime[3], '-', endTime[4], endTime[5], '-', endTime[6], endTime[7], ' ', endTime[8], endTime[9], ':', endTime[10], endTime[11], ':', endTime[12], endTime[13] };
                if (DateTime.TryParse(new String(chars), out DateTime dt))
                {
                    return dt;
                }
            }

            return DateTime.Now;
        }

        public XResult<WithdrawOrderListQueryResponseV1> QueryWithdrawOrderList(WithdrawOrderListQueryRequestV1 request)
        {
            if (request == null)
            {
                return new XResult<WithdrawOrderListQueryResponseV1>(null, ErrorCode.INVALID_ARGUMENT);
            }

            if (!request.IsValid)
            {
                return new XResult<WithdrawOrderListQueryResponseV1>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentException(request.ErrorMessage));
            }

            var q = _withdrawOrderRepository.QueryProvider.Where(x => x.PayeeId == request.PayeeId);

            var successCount = q.Where(x => x.Status == WithdrawBindCardStatus.SUCCESS.ToString()).Count();
            var successAmount = q.Where(x => x.Status == WithdrawBindCardStatus.SUCCESS.ToString()).Select(x => x.Amount).Sum();

            if (request.Status.HasValue())
            {
                q = from t0 in q
                    where t0.Status == request.Status
                    select t0;
            }

            if (request.Keyword.HasValue())
            {
                String kw = request.Keyword.Trim();

                q = from t0 in q
                    where t0.OutTradeNo == kw || t0.Remark.Contains(kw) || t0.Status.Contains(kw)
                    select t0;
            }

            if (request.From != null)
            {
                var fromDate = request.From.Value.Date;
                q = from t0 in q
                    where t0.ApplyTime >= fromDate
                    select t0;
            }

            if (request.To != null)
            {
                var toDate = request.To.Value.Date.AddDays(1).Date;
                q = from t0 in q
                    where t0.ApplyTime < toDate
                    select t0;
            }

            try
            {
                var ds = from x in q
                         select new WithdrawOrderListQueryItem()
                         {
                             Id = x.Id.ToString(),
                             Amount = x.Amount,
                             Status = x.Status,
                             OutTradeNo = x.OutTradeNo,
                             ApplyTime = x.ApplyTime,
                             CompleteTime = x.CompleteTime,
                             Remark = x.Remark
                         };

                var result = new PagedList<WithdrawOrderListQueryItem>(ds, request.PageIndex, request.PageSize);
                return new XResult<WithdrawOrderListQueryResponseV1>(new WithdrawOrderListQueryResponseV1()
                {
                    Orders = result,
                    SuccessCount = successCount,
                    SuccessAmount = successAmount,
                    PageIndex = result.PageInfo.PageIndex,
                    PageCount = result.PageInfo.PageCount,
                    Status = CommonStatus.SUCCESS.ToString(),
                    Msg = CommonStatus.SUCCESS.GetDescription()
                });
            }
            catch (Exception ex)
            {
                return new XResult<WithdrawOrderListQueryResponseV1>(null, ErrorCode.DB_QUERY_FAILED, ex);
            }
        }

        public XResult<PersonalWithdrawResultPullResponseV1> PullWithdrawResult(PersonalWithdrawResultPullRequestV1 request)
        {
            if (request == null)
            {
                return new XResult<PersonalWithdrawResultPullResponseV1>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentNullException(nameof(request)));
            }

            if (!request.IsValid)
            {
                return new XResult<PersonalWithdrawResultPullResponseV1>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentException(request.ErrorMessage));
            }

            var requestHash = $"PullWithdrawResult".GetHashCode();

            if (_lockProvider.Exists(requestHash))
            {
                return new XResult<PersonalWithdrawResultPullResponseV1>(null, ErrorCode.SUBMIT_REPEAT);
            }

            try
            {
                var unprocessedOrders = (from t0 in _withdrawOrderRepository.QueryProvider
                                         where t0.Status != WithdrawOrderStatus.SUCCESS.ToString()
                                         && t0.Status != WithdrawOrderStatus.FAILURE.ToString()
                                         select t0).OrderBy(x => x.ApplyTime).Take(request.Count).ToList();

                if (unprocessedOrders == null || unprocessedOrders.Count == 0)
                {
                    return new XResult<PersonalWithdrawResultPullResponseV1>(new PersonalWithdrawResultPullResponseV1()
                    {
                        SuccessCount = 0
                    });
                }

                Int32 successCount = 0;

                foreach (var order in unprocessedOrders)
                {
                    var queryResult = QueryWithdrawOrder(new WithdrawOrderQueryRequestV1()
                    {
                        AppId = request.AppId,
                        OutTradeNo = order.OutTradeNo,
                        QueryMode = "PULL"
                    });

                    if (queryResult.Success && queryResult.Value != null)
                    {
                        successCount++;
                    }
                }

                return new XResult<PersonalWithdrawResultPullResponseV1>(new PersonalWithdrawResultPullResponseV1()
                {
                    SuccessCount = successCount
                });
            }
            catch (Exception ex)
            {
                return new XResult<PersonalWithdrawResultPullResponseV1>(null, ErrorCode.DB_QUERY_FAILED, ex);
            }
            finally
            {
                _lockProvider.UnLock(requestHash);
            }
        }

        private Decimal GetCustomerWithdrawFee(Decimal amount)
        {
            return 0;
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

        private String GetWithdrawOrderStatusDescription(String status)
        {
            switch (status)
            {
                case nameof(WithdrawOrderStatus.APPLY):
                    return WithdrawOrderStatus.APPLY.GetDescription();
                case nameof(WithdrawOrderStatus.PROCESSING):
                    return WithdrawOrderStatus.PROCESSING.GetDescription();
                case nameof(WithdrawOrderStatus.SUCCESS):
                    return WithdrawOrderStatus.SUCCESS.GetDescription();
                case nameof(WithdrawOrderStatus.FAILURE):
                    return WithdrawOrderStatus.FAILURE.GetDescription();
            }

            return "未知状态";
        }
    }
}
