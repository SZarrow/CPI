using System;
using System.Linq;
using CPI.Common;
using CPI.Common.Domain.SettleDomain.Bill99;
using CPI.Common.Domain.SettleDomain.Bill99.v1_0;
using CPI.Common.Exceptions;
using CPI.Common.Models;
using CPI.Config;
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

                //开户成功之后要更新openid
                if (execResult.Value.ResponseCode == "0000")
                {
                    newAccount.Status = PersonalInfoRegisterStatus.SUCCESS.ToString();
                    newAccount.OpenId = execResult.Value.uId;

                    _personalSubAccountRepository.Update(newAccount);
                    var updateResult = _personalSubAccountRepository.SaveChanges();
                    if (!updateResult.Success)
                    {
                        _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, traceMethod, "更新开户信息的OpenId失败", updateResult.FirstException, newAccount);
                        return new XResult<PersonalRegisterResponseV1>(null, ErrorCode.DB_UPDATE_FAILED, new DbUpdateException("更新开户信息失败"));
                    }
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
                UserId = execResult.Value.uId,
                BankCode = execResult.Value.bankId,
                BankName = execResult.Value.bankName,
                CardType = execResult.Value.cardType,
                RequestId = execResult.Value.requestId,
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

            var lockHash = $"ApplyBindCard:{request.UserId}".GetHashCode();

            throw new NotImplementedException();
        }

        public XResult<PersonalWithdrawBindCardResponseV1> WithdrawBindCard(PersonalWithdrawBindCardRequestV1 request)
        {
            throw new NotImplementedException();
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
