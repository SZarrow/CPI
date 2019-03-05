using System;
using System.Collections.Generic;
using System.Linq;
using CPI.Common;
using CPI.Common.Domain.AgreePay;
using CPI.Common.Domain.AgreePay.YeePay;
using CPI.Common.Exceptions;
using CPI.Common.Models;
using CPI.Config;
using CPI.IData.BaseRepositories;
using CPI.IService.AgreePay;
using CPI.Providers;
using CPI.Utils;
using Lotus.Core;
using Lotus.Core.Collections;
using Lotus.Logging;

namespace CPI.Services.AgreePay
{
    public class YeePayAgreementPaymentService : IYeePayAgreementPaymentService
    {
        private static readonly LockProvider _lockProvider = new LockProvider();
        private static readonly ILogger _logger = LogManager.GetLogger();

        private readonly IAgreePayBankCardBindInfoRepository _bankCardBindInfoRepository = null;
        private readonly IAgreePayBankCardInfoRepository _bankCardInfoRepository = null;
        private readonly IPayOrderRepository _payOrderRepository = null;

        public XResult<YeePayAgreePayApplyResponse> Apply(YeePayAgreePayApplyRequest request)
        {
            if (request == null)
            {
                return new XResult<YeePayAgreePayApplyResponse>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentNullException(nameof(request)));
            }

            if (!request.IsValid)
            {
                return new XResult<YeePayAgreePayApplyResponse>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentException(request.ErrorMessage));
            }

            String service = $"{this.GetType().FullName}.Apply(...)";

            var requestHash = $"apply:{request.PayerId}.{request.BankCardNo}".GetHashCode();

            if (_lockProvider.Exists(requestHash))
            {
                return new XResult<YeePayAgreePayApplyResponse>(null, ErrorCode.SUBMIT_REPEAT);
            }

            try
            {
                if (!_lockProvider.Lock(requestHash))
                {
                    return new XResult<YeePayAgreePayApplyResponse>(null, ErrorCode.SUBMIT_REPEAT);
                }

                DateTime applyTime = DateTime.Now;

                // 如果未保存绑卡信息则添加到数据库
                var existedBankCard = _bankCardInfoRepository.QueryProvider.FirstOrDefault(x => x.IDCardNo == request.IDCardNo && x.BankCardNo == request.BankCardNo);
                if (existedBankCard == null)
                {
                    // 先将绑卡的银行卡数据入库
                    var bankCardInfo = new AgreePayBankCardInfo()
                    {
                        Id = IDGenerator.GenerateID(),
                        AppId = request.AppId,
                        RealName = request.RealName,
                        IDCardNo = request.IDCardNo,
                        BankCardNo = request.BankCardNo,
                        Mobile = request.Mobile,
                        BankCode = request.BankCode,
                        UpdateTime = applyTime
                    };

                    _bankCardInfoRepository.Add(bankCardInfo);
                    var saveResult = _bankCardInfoRepository.SaveChanges();
                    if (!saveResult.Success)
                    {
                        _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(_bankCardInfoRepository)}.SaveChanges()", "快钱协议支付：保存绑卡信息失败", saveResult.FirstException, bankCardInfo);
                        return new XResult<YeePayAgreePayApplyResponse>(null, ErrorCode.DB_UPDATE_FAILED, saveResult.FirstException);
                    }
                }

                String traceMethod = $"{nameof(YeePayAgreePayUtil)}.Execute(...)";

                _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.OK.ToString(), service, traceMethod, LogPhase.BEGIN, "开始调用易宝协议支付接口", request);

                var execResult = YeePayAgreePayUtil.Execute<RawYeePayApplyBindCardRequest, RawYeePayApplyBindCardResult>("/rest/v1.0/paperorder/unified/auth/request", new RawYeePayApplyBindCardRequest()
                {
                    merchantno = GlobalConfig.YeePay_AgreePay_MerchantNo,
                    requestno = request.OutTradeNo,
                    identityid = request.PayerId,
                    identitytype = "USER_ID",
                    idcardno = request.IDCardNo,
                    cardno = request.BankCardNo,
                    idcardtype = "ID",
                    username = request.RealName,
                    phone = request.Mobile,
                    issms = "true",
                    requesttime = applyTime.ToString("yyyy/MM/dd HH:mm:ss"),
                    authtype = "COMMON_FOUR"
                });

                _logger.Trace(TraceType.BLL.ToString(), (execResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), service, traceMethod, LogPhase.ACTION, "结束调用易宝协议支付接口");

                if (!execResult.Success)
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, traceMethod, "协议支付失败", execResult.FirstException, execResult);
                    return new XResult<YeePayAgreePayApplyResponse>(null, ErrorCode.DEPENDENT_API_CALL_FAILED, new RequestException(execResult.ErrorMessage));
                }

                var respResult = execResult.Value;
                if (respResult.status != "TO_VALIDATE")
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, "respResult", $"{respResult.errorcode}:{respResult.errormsg}", null, respResult);

                    _bankCardInfoRepository.Remove(existedBankCard);
                    var removeResult = _bankCardInfoRepository.SaveChanges();
                    if (!removeResult.Success)
                    {
                        _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, "removeResult", "无法删除申请绑卡失败的记录", removeResult.FirstException, existedBankCard);
                    }
                    else
                    {
                        _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.OK.ToString(), service, "removeResult", LogPhase.ACTION, "成功删除申请绑卡失败的记录", existedBankCard);
                    }

                    return new XResult<YeePayAgreePayApplyResponse>(null, ErrorCode.DEPENDENT_API_CALL_FAILED, new RemoteException(respResult.errormsg));
                }

                var resp = new YeePayAgreePayApplyResponse()
                {
                    OutTradeNo = respResult.requestno,
                    ApplyTime = applyTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    PayerId = request.PayerId,
                    Status = CommonStatus.SUCCESS.ToString(),
                    Msg = $"申请{CommonStatus.SUCCESS.GetDescription()}"
                };

                return new XResult<YeePayAgreePayApplyResponse>(resp);
            }
            finally
            {
                _lockProvider.UnLock(requestHash);
            }
        }

        public XResult<YeePayAgreePayBindCardResponse> BindCard(YeePayAgreePayBindCardRequest request)
        {
            if (request == null)
            {
                return new XResult<YeePayAgreePayBindCardResponse>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentNullException(nameof(request)));
            }

            if (!request.IsValid)
            {
                return new XResult<YeePayAgreePayBindCardResponse>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentException(request.ErrorMessage));
            }

            String service = $"{this.GetType().FullName}.BindCard(...)";

            var requestHash = $"bindcard:{request.PayerId}.{request.BankCardNo}".GetHashCode();

            if (_lockProvider.Exists(requestHash))
            {
                return new XResult<YeePayAgreePayBindCardResponse>(null, ErrorCode.SUBMIT_REPEAT);
            }

            try
            {
                if (!_lockProvider.Lock(requestHash))
                {
                    return new XResult<YeePayAgreePayBindCardResponse>(null, ErrorCode.SUBMIT_REPEAT);
                }

                // 根据付款人Id、银行卡号和通道判断绑卡数据是否已存在
                var existsBindInfo = _bankCardBindInfoRepository.Exists(x => x.PayerId == request.PayerId && x.BankCardNo == request.BankCardNo && x.PayChannelCode == GlobalConfig.X99BILL_PAYCHANNEL_CODE);
                if (existsBindInfo)
                {
                    return new XResult<YeePayAgreePayBindCardResponse>(null, ErrorCode.INFO_EXISTED, new ArgumentException("绑卡信息已存在"));
                }

                // 从已保存的绑卡数据中查出缺失的字段数据
                var existedBankCardInfo = _bankCardInfoRepository.QueryProvider.FirstOrDefault(x => x.BankCardNo == request.BankCardNo);
                if (existedBankCardInfo == null)
                {
                    return new XResult<YeePayAgreePayBindCardResponse>(null, ErrorCode.INFO_NOT_EXIST, new ArgumentException("该付款人的申请绑卡记录不存在"));
                }
                else
                {
                    request.Mobile = existedBankCardInfo.Mobile;
                }

                var tradeTime = DateTime.Now;

                String callMethod = $"{nameof(YeePayAgreePayUtil)}.Execute(...)";
                _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.OK.ToString(), service, callMethod, LogPhase.BEGIN, $"开始调用{callMethod}", new Object[] { ApiConfig.Bill99_AgreePay_BindCard_RequestUrl, request });

                var result = YeePayAgreePayUtil.Execute<RawYeePayBindCardRequest, RawYeePayBindCardResult>("/rest/v1.0/paperorder/auth/confirm", new RawYeePayBindCardRequest()
                {
                    merchantno = GlobalConfig.YeePay_AgreePay_MerchantNo,
                    requestno = request.OutTradeNo,
                    validatecode = request.SmsValidCode
                });

                _logger.Trace(TraceType.BLL.ToString(), (result.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), service, callMethod, LogPhase.END, $"完成调用{callMethod}", result.Value);

                if (!result.Success)
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, callMethod, "绑卡失败", result.FirstException, new Object[] { request, result.Value });
                    return new XResult<YeePayAgreePayBindCardResponse>(null, ErrorCode.DEPENDENT_API_CALL_FAILED, result.FirstException);
                }

                if (result.Value == null)
                {
                    return new XResult<YeePayAgreePayBindCardResponse>(null, ErrorCode.REMOTE_RETURN_NOTHING);
                }

                var respResult = result.Value;
                if (respResult.status != "BIND_SUCCESS")
                {
                    //如果绑卡失败要删除之前的申请绑卡时保存的银行卡信息
                    _bankCardInfoRepository.Remove(existedBankCardInfo);
                    var removeExistedBankCardInfoResult = _bankCardInfoRepository.SaveChanges();
                    if (!removeExistedBankCardInfoResult.Success)
                    {
                        _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(_bankCardInfoRepository)}.SaveChanges()", "删除绑卡失败的银行卡信息失败", removeExistedBankCardInfoResult.FirstException, existedBankCardInfo);
                    }

                    return new XResult<YeePayAgreePayBindCardResponse>(null, ErrorCode.DEPENDENT_API_CALL_FAILED, new RemoteException(respResult.errormsg));
                }

                // 绑卡成功后数据入库
                var bindInfo = new AgreePayBankCardBindInfo()
                {
                    Id = IDGenerator.GenerateID(),
                    AppId = request.AppId,
                    PayerId = request.PayerId,
                    BankCardId = existedBankCardInfo.Id,
                    OutTradeNo = request.OutTradeNo,
                    BankCardNo = request.BankCardNo,
                    PayChannelCode = GlobalConfig.YEEPAY_PAYCHANNEL_CODE,
                    BindStatus = nameof(BankCardBindStatus.BOUND),
                    ApplyTime = tradeTime
                };

                _bankCardBindInfoRepository.Add(bindInfo);
                var saveResult = _bankCardBindInfoRepository.SaveChanges();
                if (!saveResult.Success)
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(_bankCardBindInfoRepository)}.SaveChanges()", "保存绑卡信息失败", saveResult.FirstException, bindInfo);
                    return new XResult<YeePayAgreePayBindCardResponse>(null, ErrorCode.DB_UPDATE_FAILED, saveResult.FirstException);
                }

                var resp = new YeePayAgreePayBindCardResponse()
                {
                    PayerId = request.PayerId,
                    OutTradeNo = respResult.requestno,
                    BindTime = tradeTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    Status = CommonStatus.SUCCESS.ToString(),
                    Msg = $"绑卡{CommonStatus.SUCCESS.GetDescription()}"
                };

                return new XResult<YeePayAgreePayBindCardResponse>(resp);
            }
            finally
            {
                _lockProvider.UnLock(requestHash);
            }
        }

        public XResult<YeePayAgreePayPaymentResponse> Pay(YeePayAgreePayPaymentRequest request)
        {
            if (request == null)
            {
                return new XResult<YeePayAgreePayPaymentResponse>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentNullException(nameof(request)));
            }

            if (!request.IsValid)
            {
                return new XResult<YeePayAgreePayPaymentResponse>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentException(request.ErrorMessage));
            }

            if (request.Amount < GlobalConfig.YeePay_AgreePay_PayMinAmount)
            {
                return new XResult<YeePayAgreePayPaymentResponse>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentException($"支付总金额必须大于{GlobalConfig.YeePay_AgreePay_PayMinAmount.ToString()}"));
            }

            String service = $"{this.GetType().FullName}.Pay(...)";

            var requestHash = $"pay:{request.OutTradeNo}".GetHashCode();

            if (_lockProvider.Exists(requestHash))
            {
                return new XResult<YeePayAgreePayPaymentResponse>(null, ErrorCode.SUBMIT_REPEAT);
            }

            try
            {
                if (!_lockProvider.Lock(requestHash))
                {
                    return new XResult<YeePayAgreePayPaymentResponse>(null, ErrorCode.SUBMIT_REPEAT);
                }

                // 保证外部交易号不重复
                var existsOutTradeNo = _payOrderRepository.Exists(x => x.AppId == request.AppId && x.OutTradeNo == request.OutTradeNo);
                if (existsOutTradeNo)
                {
                    return new XResult<YeePayAgreePayPaymentResponse>(null, ErrorCode.OUT_TRADE_NO_EXISTED);
                }

                //生成全局唯一的ID号
                Int64 newId = IDGenerator.GenerateID();
                String tradeNo = newId.ToString();
                var tradeTime = DateTime.Now;

                // 添加支付单记录
                var newOrder = new PayOrder()
                {
                    Id = newId,
                    AppId = request.AppId,
                    PayerId = request.PayerId,
                    OutTradeNo = request.OutTradeNo,
                    TradeNo = tradeNo,
                    PayAmount = request.Amount,
                    BankCardNo = request.BankCardNo,
                    PayChannelCode = GlobalConfig.YEEPAY_PAYCHANNEL_CODE,
                    PayStatus = PayStatus.APPLY.ToString(),
                    PayType = PayType.AGREEMENTPAY.ToString(),
                    CreateTime = tradeTime
                };

                _payOrderRepository.Add(newOrder);
                var saveResult = _payOrderRepository.SaveChanges();
                if (!saveResult.Success)
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(_payOrderRepository)}.SaveChanges()", "支付单保存失败", saveResult.FirstException, newOrder);
                    return new XResult<YeePayAgreePayPaymentResponse>(null, ErrorCode.DB_UPDATE_FAILED, saveResult.FirstException);
                }

                String callMethod = $"{nameof(YeePayAgreePayUtil)}.Execute(...)";
                _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.OK.ToString(), service, callMethod, LogPhase.BEGIN, $"开始调用{callMethod}", new Object[] { ApiConfig.YeePay_AgreePay_RequestUrl, request });

                var result = YeePayAgreePayUtil.Execute<RawYeePayAgreePayPaymentRequest, RawYeePayAgreePayPaymentResult>("/rest/v1.0/paperorder/unified/pay", new RawYeePayAgreePayPaymentRequest()
                {
                    merchantno = GlobalConfig.YeePay_AgreePay_MerchantNo,
                    requestno = request.OutTradeNo,
                    issms = "false",
                    identityid = request.PayerId,
                    identitytype = "USER_ID",
                    cardtop = request.BankCardNo.Substring(0, 6),
                    cardlast = request.BankCardNo.Substring(request.BankCardNo.Length - 4, 4),
                    amount = request.Amount.ToString(),
                    productname = "消费支付",
                    requesttime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    terminalno = request.TerminalNo
                });

                _logger.Trace(TraceType.BLL.ToString(), (result.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), service, callMethod, LogPhase.END, $"完成调用{callMethod}", result.Value);

                if (!result.Success)
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, callMethod, "支付失败", result.FirstException, result);
                    return new XResult<YeePayAgreePayPaymentResponse>(null, ErrorCode.DEPENDENT_API_CALL_FAILED, result.FirstException);
                }

                if (result.Value == null)
                {
                    return new XResult<YeePayAgreePayPaymentResponse>(null, ErrorCode.REMOTE_RETURN_NOTHING);
                }

                var respResult = result.Value;
                if (respResult.status != "PROCESSING")
                {
                    newOrder.PayStatus = PayStatus.FAILURE.ToString();
                    newOrder.UpdateTime = DateTime.Now;
                    _payOrderRepository.Update(newOrder);
                    saveResult = _payOrderRepository.SaveChanges();
                    if (!saveResult.Success)
                    {
                        _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(_payOrderRepository)}.SaveChanges()", "更新协议支付结果失败", saveResult.FirstException, newOrder);
                    }
                }

                var resp = new YeePayAgreePayPaymentResponse()
                {
                    OutTradeNo = respResult.requestno,
                    TradeNo = tradeNo,
                    PayTime = tradeTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    Status = PayStatus.PROCESSING.ToString(),
                    Msg = PayStatus.PROCESSING.GetDescription()
                };

                return new XResult<YeePayAgreePayPaymentResponse>(resp);
            }
            finally
            {
                _lockProvider.UnLock(requestHash);
            }
        }

        public XResult<Int32> Pull(Int32 count)
        {
            throw new NotImplementedException();
        }

        public XResult<PagedList<CPIAgreePayQueryResult>> Query(CPIAgreePayQueryRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
