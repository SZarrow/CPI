using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CPI.Common;
using CPI.Common.Domain.AgreePay;
using CPI.Common.Domain.Common;
using CPI.Common.Domain.SettleDomain.Bill99;
using CPI.Common.Domain.SettleDomain.Bill99.v1_0;
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
using Lotus.Payment.Bill99;
using Lotus.Payment.Bill99.Domain;

namespace CPI.Services.AgreePay
{
    public class Bill99AgreementPaymentService : IAgreementPaymentService
    {
        private static readonly LockProvider _lockProvider = new LockProvider();
        private static readonly ILogger _logger = LogManager.GetLogger();
        private static readonly AgreementPaymentApi _api = CreateAgreementPaymentApi();

        private readonly IAgreePayBankCardBindInfoRepository _bankCardBindInfoRepository = null;
        private readonly IAgreePayBankCardInfoRepository _bankCardInfoRepository = null;
        private readonly IPayOrderRepository _payOrderRepository = null;
        private readonly IAllotAmountOrderRepository _allotAmountOrderRepository = null;

        private static AgreementPaymentApi CreateAgreementPaymentApi()
        {
            var factory = XDI.Resolve<IHttpClientFactory>();
            var client = factory.CreateClient("AgreePaymentApiHttpClient");
            return new AgreementPaymentApi(client, GlobalConfig.X99bill_AgreePay_Hehua_MerchantId, GlobalConfig.X99bill_AgreePay_Hehua_TerminalId);
        }

        public XResult<CPIAgreePayApplyResponse> Apply(CPIAgreePayApplyRequest request)
        {
            if (request == null)
            {
                return new XResult<CPIAgreePayApplyResponse>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentNullException(nameof(request)));
            }

            if (!request.IsValid)
            {
                return new XResult<CPIAgreePayApplyResponse>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentException(request.ErrorMessage));
            }

            String service = $"{this.GetType().FullName}.Apply(...)";

            var requestHash = $"apply:{request.PayerId}.{request.BankCardNo}".GetHashCode();

            if (_lockProvider.Exists(requestHash))
            {
                return new XResult<CPIAgreePayApplyResponse>(null, ErrorCode.SUBMIT_REPEAT);
            }

            try
            {
                if (!_lockProvider.Lock(requestHash))
                {
                    return new XResult<CPIAgreePayApplyResponse>(null, ErrorCode.SUBMIT_REPEAT);
                }

                // 如果未保存绑卡信息则添加到数据库
                var existsBankCard = _bankCardInfoRepository.Exists(x => x.IDCardNo == request.IDCardNo && x.BankCardNo == request.BankCardNo);
                if (!existsBankCard)
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
                        UpdateTime = DateTime.Now
                    };

                    _bankCardInfoRepository.Add(bankCardInfo);
                    var saveResult = _bankCardInfoRepository.SaveChanges();
                    if (!saveResult.Success)
                    {
                        _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(_bankCardInfoRepository)}.SaveChanges()", "快钱协议支付：保存绑卡信息失败", saveResult.FirstException, bankCardInfo);
                        return new XResult<CPIAgreePayApplyResponse>(null, ErrorCode.DB_UPDATE_FAILED, saveResult.FirstException);
                    }
                }

                var tradeTime = DateTime.Now;

                // 入库成功之后才开始调第三方申请接口
                var applyRequest = new AgreementApplyRequest()
                {
                    Version = "1.0",
                    IndAuthContent = new IndAuthRequestContent()
                    {
                        BindType = "0",
                        CustomerId = request.PayerId,
                        ExternalRefNumber = request.OutTradeNo,
                        Pan = request.BankCardNo,
                        CardHolderName = request.RealName,
                        CardHolderId = request.IDCardNo,
                        PhoneNO = request.Mobile
                    }
                };

                String callMethod = $"{nameof(_api)}.AgreementApply(...)";
                _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.OK.ToString(), service, callMethod, LogPhase.BEGIN, $"开始调用{callMethod}", new Object[] { ApiConfig.Bill99_AgreePay_ApplyBindCard_RequestUrl, applyRequest });

                var result = _api.AgreementApply(ApiConfig.Bill99_AgreePay_ApplyBindCard_RequestUrl, applyRequest);

                _logger.Trace(TraceType.BLL.ToString(), (result.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), service, callMethod, LogPhase.END, $"完成调用{callMethod}", result.Value);

                if (!result.Success)
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, callMethod, "申请绑卡失败", result.FirstException, applyRequest);
                    return new XResult<CPIAgreePayApplyResponse>(null, ErrorCode.DEPENDENT_API_CALL_FAILED, result.FirstException);
                }

                if (result.Value == null)
                {
                    return new XResult<CPIAgreePayApplyResponse>(null, ErrorCode.REMOTE_RETURN_NOTHING);
                }

                if (result.Value.IndAuthContent == null)
                {
                    return result.Value.ErrorMsgContent != null
                        ? new XResult<CPIAgreePayApplyResponse>(null, ErrorCode.DEPENDENT_API_CALL_FAILED, new RemoteException(result.Value.ErrorMsgContent.ErrorMessage))
                        : new XResult<CPIAgreePayApplyResponse>(null, ErrorCode.REMOTE_RETURN_NOTHING);
                }

                var respContent = result.Value.IndAuthContent;
                if (respContent.ResponseCode != "00")
                {
                    return new XResult<CPIAgreePayApplyResponse>(null, ErrorCode.DEPENDENT_API_CALL_FAILED, new RemoteException(respContent.ResponseTextMessage));
                }

                var resp = new CPIAgreePayApplyResponse()
                {
                    PayerId = respContent.CustomerId,
                    OutTradeNo = respContent.ExternalRefNumber,
                    ApplyToken = respContent.Token,
                    ApplyTime = tradeTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    Status = CommonStatus.SUCCESS.ToString(),
                    Msg = CommonStatus.SUCCESS.GetDescription()
                };

                return new XResult<CPIAgreePayApplyResponse>(resp);
            }
            finally
            {
                _lockProvider.UnLock(requestHash);
            }
        }

        public XResult<CPIAgreePayBindCardResponse> BindCard(CPIAgreePayBindCardRequest request)
        {
            if (request == null)
            {
                return new XResult<CPIAgreePayBindCardResponse>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentNullException(nameof(request)));
            }

            if (!request.IsValid)
            {
                return new XResult<CPIAgreePayBindCardResponse>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentException(request.ErrorMessage));
            }

            String service = $"{this.GetType().FullName}.BindCard(...)";

            var requestHash = $"bindcard:{request.PayerId}.{request.BankCardNo}".GetHashCode();

            if (_lockProvider.Exists(requestHash))
            {
                return new XResult<CPIAgreePayBindCardResponse>(null, ErrorCode.SUBMIT_REPEAT);
            }

            try
            {
                if (!_lockProvider.Lock(requestHash))
                {
                    return new XResult<CPIAgreePayBindCardResponse>(null, ErrorCode.SUBMIT_REPEAT);
                }

                // 根据付款人Id、银行卡号和通道判断绑卡数据是否已存在
                var existsBindInfo = _bankCardBindInfoRepository.Exists(x => x.PayerId == request.PayerId && x.BankCardNo == request.BankCardNo && x.PayChannelCode == GlobalConfig.X99BILL_PAYCHANNEL_CODE);
                if (existsBindInfo)
                {
                    return new XResult<CPIAgreePayBindCardResponse>(null, ErrorCode.INFO_EXISTED, new ArgumentException("绑卡信息已存在"));
                }

                // 从已保存的绑卡数据中查出缺失的字段数据
                var existedBankCardInfo = _bankCardInfoRepository.QueryProvider.FirstOrDefault(x => x.BankCardNo == request.BankCardNo);
                if (existedBankCardInfo == null)
                {
                    return new XResult<CPIAgreePayBindCardResponse>(null, ErrorCode.INFO_NOT_EXIST, new ArgumentException("该付款人的申请绑卡记录不存在"));
                }
                else
                {
                    request.Mobile = existedBankCardInfo.Mobile;
                }

                var tradeTime = DateTime.Now;

                // 调用第三方绑卡接口进行绑卡
                var bindRequest = new AgreementBindRequest()
                {
                    Version = "1.0",
                    IndAuthDynVerifyContent = new IndAuthDynVerifyRequestContent()
                    {
                        BindType = "0",
                        CustomerId = request.PayerId,
                        ExternalRefNumber = request.OutTradeNo,
                        Pan = request.BankCardNo,
                        PhoneNO = request.Mobile,
                        Token = request.ApplyToken,
                        ValidCode = request.SmsValidCode
                    }
                };

                String callMethod = $"{_api.GetType().FullName}.AgreementVerify(...)";
                _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.OK.ToString(), service, callMethod, LogPhase.BEGIN, $"开始调用{callMethod}", new Object[] { ApiConfig.Bill99_AgreePay_BindCard_RequestUrl, bindRequest });

                var result = _api.AgreementVerify(ApiConfig.Bill99_AgreePay_BindCard_RequestUrl, bindRequest);

                _logger.Trace(TraceType.BLL.ToString(), (result.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), service, callMethod, LogPhase.END, $"完成调用{callMethod}", result.Value);

                if (!result.Success)
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, callMethod, "绑卡失败", result.FirstException, bindRequest);
                    return new XResult<CPIAgreePayBindCardResponse>(null, ErrorCode.DEPENDENT_API_CALL_FAILED, result.FirstException);
                }

                if (result.Value == null)
                {
                    return new XResult<CPIAgreePayBindCardResponse>(null, ErrorCode.REMOTE_RETURN_NOTHING);
                }

                if (result.Value.IndAuthDynVerifyContent == null)
                {
                    return result.Value.ErrorMsgContent != null
                        ? new XResult<CPIAgreePayBindCardResponse>(null, ErrorCode.DEPENDENT_API_CALL_FAILED, new RemoteException(result.Value.ErrorMsgContent.ErrorMessage))
                        : new XResult<CPIAgreePayBindCardResponse>(null, ErrorCode.REMOTE_RETURN_NOTHING);
                }

                var respContent = result.Value.IndAuthDynVerifyContent;
                if (respContent.ResponseCode != "00")
                {
                    //如果绑卡失败要删除之前的申请绑卡时保存的银行卡信息
                    _bankCardInfoRepository.Remove(existedBankCardInfo);
                    var removeExistedBankCardInfoResult = _bankCardInfoRepository.SaveChanges();
                    if (!removeExistedBankCardInfoResult.Success)
                    {
                        _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(_bankCardInfoRepository)}.SaveChanges()", "删除绑卡失败的银行卡信息失败", removeExistedBankCardInfoResult.FirstException, existedBankCardInfo);
                    }

                    return new XResult<CPIAgreePayBindCardResponse>(null, ErrorCode.DEPENDENT_API_CALL_FAILED, new RemoteException(respContent.ResponseTextMessage));
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
                    PayToken = respContent.PayToken,
                    PayChannelCode = GlobalConfig.X99BILL_PAYCHANNEL_CODE,
                    BindStatus = nameof(BankCardBindStatus.BOUND),
                    ApplyTime = DateTime.Now
                };

                _bankCardBindInfoRepository.Add(bindInfo);
                var saveResult = _bankCardBindInfoRepository.SaveChanges();
                if (!saveResult.Success)
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(_bankCardBindInfoRepository)}.SaveChanges()", "保存绑卡信息失败", saveResult.FirstException, bindInfo);
                    return new XResult<CPIAgreePayBindCardResponse>(null, ErrorCode.DB_UPDATE_FAILED, saveResult.FirstException);
                }

                var resp = new CPIAgreePayBindCardResponse()
                {
                    PayerId = respContent.CustomerId,
                    PayToken = respContent.PayToken,
                    BindTime = tradeTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    Status = CommonStatus.SUCCESS.ToString(),
                    Msg = CommonStatus.SUCCESS.GetDescription()
                };

                return new XResult<CPIAgreePayBindCardResponse>(resp);
            }
            finally
            {
                _lockProvider.UnLock(requestHash);
            }
        }

        public XResult<CPIAgreePayPaymentResponse> Pay(CPIAgreePayPaymentRequest request)
        {
            if (request == null)
            {
                return new XResult<CPIAgreePayPaymentResponse>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentNullException(nameof(request)));
            }

            if (!request.IsValid)
            {
                return new XResult<CPIAgreePayPaymentResponse>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentException(request.ErrorMessage));
            }

            if (request.Amount < GlobalConfig.X99bill_AgreePay_PayMinAmount)
            {
                return new XResult<CPIAgreePayPaymentResponse>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentException($"支付总金额必须大于{GlobalConfig.X99bill_AgreePay_PayMinAmount.ToString()}"));
            }

            var parseSharingInfoResult = JsonUtil.DeserializeObject<SharingInfo>(request.SharingInfo);
            if (!parseSharingInfoResult.Success)
            {
                return new XResult<CPIAgreePayPaymentResponse>(null, ErrorCode.DESERIALIZE_FAILED, new ArgumentException("解析SharingInfo参数失败"));
            }

            var sharingInfo = parseSharingInfoResult.Value;

            String service = $"{this.GetType().FullName}.Pay(...)";

            var requestHash = $"pay:{request.OutTradeNo}".GetHashCode();

            if (_lockProvider.Exists(requestHash))
            {
                return new XResult<CPIAgreePayPaymentResponse>(null, ErrorCode.SUBMIT_REPEAT);
            }

            try
            {
                if (!_lockProvider.Lock(requestHash))
                {
                    return new XResult<CPIAgreePayPaymentResponse>(null, ErrorCode.SUBMIT_REPEAT);
                }

                // 保证外部交易号不重复
                var existsOutTradeNo = _payOrderRepository.Exists(x => x.AppId == request.AppId && x.OutTradeNo == request.OutTradeNo);
                if (existsOutTradeNo)
                {
                    return new XResult<CPIAgreePayPaymentResponse>(null, ErrorCode.OUT_TRADE_NO_EXISTED);
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
                    PayChannelCode = GlobalConfig.X99BILL_PAYCHANNEL_CODE,
                    PayStatus = PayStatus.APPLY.ToString(),
                    PayType = PayType.AGREEMENTPAY.ToString(),
                    CreateTime = tradeTime
                };

                _payOrderRepository.Add(newOrder);
                var saveResult = _payOrderRepository.SaveChanges();
                if (!saveResult.Success)
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(_payOrderRepository)}.SaveChanges()", "支付单保存失败", saveResult.FirstException, newOrder);
                    return new XResult<CPIAgreePayPaymentResponse>(null, ErrorCode.DB_UPDATE_FAILED, saveResult.FirstException);
                }

                //添加分账记录
                var allotAmountOrder = new AllotAmountOrder()
                {
                    Id = newId,
                    AppId = request.AppId,
                    PayeeId = request.PayerId,
                    TradeNo = tradeNo,
                    OutTradeNo = request.OutTradeNo,
                    TotalAmount = request.Amount,
                    FeePayerId = sharingInfo.FeePayerId,
                    SharingType = sharingInfo.SharingType == "0" ? AllotAmountType.Pay.ToString() : AllotAmountType.Refund.ToString(),
                    SharingInfo = sharingInfo.SharingData,
                    ApplyTime = tradeTime,
                    Status = AllotAmountOrderStatus.APPLY.ToString()
                };

                _allotAmountOrderRepository.Add(allotAmountOrder);
                saveResult = _allotAmountOrderRepository.SaveChanges();

                if (!saveResult.Success)
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(_allotAmountOrderRepository)}.SaveChanges()", "分账数据保存失败", saveResult.FirstException, allotAmountOrder);
                    return new XResult<CPIAgreePayPaymentResponse>(null, ErrorCode.DB_UPDATE_FAILED, saveResult.FirstException);
                }

                //构造分账参数
                ExtDate sharingExtDate = null;
                if (sharingInfo.SharingType == "0")
                {
                    //构造消费分账数据
                    var sharingDic = new Dictionary<String, String>(5);
                    sharingDic["sharingFlag"] = "1";
                    sharingDic["feeMode"] = sharingInfo.FeeMode;
                    sharingDic["feePayer"] = sharingInfo.FeePayerId;
                    sharingDic["sharingData"] = sharingInfo.SharingData;

                    sharingExtDate = new ExtDate()
                    {
                        Key = "sharingInfo",
                        Value = JsonUtil.SerializeObject(sharingDic).Value
                    };
                }
                else if (sharingInfo.SharingType == "1")
                {
                    //构造退款分账数据
                    var sharingDic = new Dictionary<String, String>(5);
                    sharingDic["sharingFlag"] = "1";
                    sharingDic["feeMode"] = sharingInfo.FeeMode;
                    sharingDic["feePayer"] = sharingInfo.FeePayerId;
                    sharingDic["sharingData"] = sharingInfo.SharingData;

                    sharingExtDate = new ExtDate()
                    {
                        Key = "refundSharingInfo",
                        Value = JsonUtil.SerializeObject(sharingDic).Value
                    };
                }

                var payRequest = new AgreementPayRequest()
                {
                    Version = "1.0",
                    TxnMsgContent = new TxnMsgRequestContent()
                    {
                        Amount = request.Amount.ToString(),
                        CustomerId = request.PayerId,
                        EntryTime = tradeTime.ToString("yyyyMMddHHmmss"),
                        ExternalRefNumber = request.OutTradeNo,
                        InteractiveStatus = "TR1",
                        NotifyUrl = request.NotifyUrl,
                        PayToken = request.PayToken,
                        SpFlag = "QPay02",
                        TxnType = "PUR",
                        ExtMap = new ExtMap()
                        {
                            ExtDates = new ExtDate[] {
                                new ExtDate() {
                                    Key = "phone",
                                    Value = request.Mobile
                                },
                                sharingExtDate
                            }
                        }
                    }
                };

                String callMethod = $"{_api.GetType().FullName}.AgreementPay(...)";
                _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.OK.ToString(), service, callMethod, LogPhase.BEGIN, $"开始调用{callMethod}", new Object[] { ApiConfig.Bill99_AgreePay_Pay_RequestUrl, payRequest });

                var result = _api.AgreementPay(ApiConfig.Bill99_AgreePay_Pay_RequestUrl, payRequest);

                _logger.Trace(TraceType.BLL.ToString(), (result.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), service, callMethod, LogPhase.END, $"完成调用{callMethod}", result.Value);

                if (!result.Success)
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, callMethod, "支付失败", result.FirstException, result);
                    UpdateAllotAmountOrderStatus(service, allotAmountOrder, AllotAmountOrderStatus.FAILURE);
                    return new XResult<CPIAgreePayPaymentResponse>(null, ErrorCode.DEPENDENT_API_CALL_FAILED, result.FirstException);
                }

                if (result.Value == null)
                {
                    UpdateAllotAmountOrderStatus(service, allotAmountOrder, AllotAmountOrderStatus.FAILURE);
                    return new XResult<CPIAgreePayPaymentResponse>(null, ErrorCode.REMOTE_RETURN_NOTHING);
                }

                if (result.Value.TxnMsgContent == null)
                {
                    UpdateAllotAmountOrderStatus(service, allotAmountOrder, AllotAmountOrderStatus.FAILURE);
                    return result.Value.ErrorMsgContent != null
                        ? new XResult<CPIAgreePayPaymentResponse>(null, ErrorCode.DEPENDENT_API_CALL_FAILED, new RemoteException(result.Value.ErrorMsgContent.ErrorMessage))
                        : new XResult<CPIAgreePayPaymentResponse>(null, ErrorCode.REMOTE_RETURN_NOTHING);
                }

                var respContent = result.Value.TxnMsgContent;
                if (respContent.ResponseCode != "00")
                {
                    UpdateAllotAmountOrderStatus(service, allotAmountOrder, AllotAmountOrderStatus.FAILURE);
                    return new XResult<CPIAgreePayPaymentResponse>(null, ErrorCode.DEPENDENT_API_CALL_FAILED, new RemoteException(respContent.ResponseTextMessage));
                }

                UpdateAllotAmountOrderStatus(service, allotAmountOrder, AllotAmountOrderStatus.SUCCESS);

                var resp = new CPIAgreePayPaymentResponse()
                {
                    OutTradeNo = respContent.ExternalRefNumber,
                    TradeNo = tradeNo,
                    PayTime = tradeTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    Status = PayStatus.SUCCESS.ToString(),
                    Msg = $"支付{PayStatus.SUCCESS.GetDescription()}"
                };

                //根据返回码计算对应的支付状态，并更新到数据库
                var payStatus = Bill99Util.GetAgreepayPayStatus(respContent.ResponseCode);
                var payOrder = (from t0 in _payOrderRepository.QueryProvider
                                where t0.AppId == request.AppId && t0.TradeNo == tradeNo
                                select t0).FirstOrDefault();

                //如果支付单存在，并且状态不是最终状态，才更新状态
                if (payOrder != null
                    && payOrder.PayStatus != PayStatus.SUCCESS.ToString()
                    && payOrder.PayStatus != PayStatus.FAILURE.ToString())
                {
                    payOrder.PayStatus = payStatus.ToString();
                    payOrder.UpdateTime = DateTime.Now;
                    _payOrderRepository.Update(payOrder);
                    saveResult = _payOrderRepository.SaveChanges();
                    if (!saveResult.Success)
                    {
                        _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(_payOrderRepository)}.SaveChanges()", "更新协议支付结果失败", saveResult.FirstException, payOrder);
                    }
                }

                return new XResult<CPIAgreePayPaymentResponse>(resp);
            }
            finally
            {
                _lockProvider.UnLock(requestHash);
            }
        }

        private void UpdateAllotAmountOrderStatus(String service, AllotAmountOrder allotAmountOrder, AllotAmountOrderStatus status)
        {
            allotAmountOrder.Status = status.ToString();
            _allotAmountOrderRepository.Update(allotAmountOrder);
            var saveResult = _allotAmountOrderRepository.SaveChanges();
            if (!saveResult.Success)
            {
                _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(_allotAmountOrderRepository)}.SaveChanges()", "更新分账状态失败", saveResult.FirstException, allotAmountOrder);
            }
        }

        public XResult<PagedList<CPIAgreePayQueryResult>> Query(CPIAgreePayQueryRequest request)
        {
            if (request == null)
            {
                return new XResult<PagedList<CPIAgreePayQueryResult>>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentNullException(nameof(request)));
            }

            if (!request.IsValid)
            {
                return new XResult<PagedList<CPIAgreePayQueryResult>>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentException(request.ErrorMessage));
            }

            var q = _payOrderRepository.QueryProvider;

            if (!String.IsNullOrWhiteSpace(request.OutTradeNo))
            {
                q = q.Where(x => x.OutTradeNo == request.OutTradeNo);
            }

            if (request.From != null)
            {
                q = q.Where(x => x.CreateTime >= request.From.Value);
            }

            if (request.To != null)
            {
                q = q.Where(x => x.CreateTime <= request.To.Value);
            }

            try
            {
                var ds = q.Select(x => new CPIAgreePayQueryResult()
                {
                    OutTradeNo = x.OutTradeNo,
                    Amount = x.PayAmount,
                    Status = x.PayStatus,
                    Msg = GetQueryStatusMsg(x.PayStatus),
                    CreateTime = x.CreateTime
                }).OrderByDescending(x => x.CreateTime);

                var result = new PagedList<CPIAgreePayQueryResult>(ds, request.PageIndex, request.PageSize);
                if (result.Exception != null)
                {
                    return new XResult<PagedList<CPIAgreePayQueryResult>>(null, ErrorCode.DB_QUERY_FAILED, result.Exception);
                }

                return new XResult<PagedList<CPIAgreePayQueryResult>>(result);
            }
            catch (Exception ex)
            {
                return new XResult<PagedList<CPIAgreePayQueryResult>>(null, ErrorCode.DB_QUERY_FAILED, ex);
            }
        }

        public XResult<Int32> Pull(Int32 count)
        {
            if (count <= 0 || count > 20)
            {
                return new XResult<Int32>(0, ErrorCode.INVALID_ARGUMENT, new ArgumentOutOfRangeException($"参数count超出范围[1,20]"));
            }

            String service = $"{this.GetType().FullName}.Pull(...)";

            var key = DateTime.Now.Date.GetHashCode();

            if (_lockProvider.Exists(key))
            {
                return new XResult<Int32>(0);
            }

            try
            {
                if (!_lockProvider.Lock(key))
                {
                    return new XResult<Int32>(0);
                }

                List<PullQueryItem> items = null;

                try
                {
                    items = (from t0 in _payOrderRepository.QueryProvider
                             where t0.PayStatus != PayStatus.FAILURE.ToString()
                             && t0.PayStatus != PayStatus.SUCCESS.ToString()
                             orderby t0.CreateTime
                             select new PullQueryItem(t0.OutTradeNo, t0.CreateTime)).ToList();
                }
                catch (Exception ex)
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, "items", "查询处理中的订单失败", ex);
                    return new XResult<Int32>(0);
                }

                if (items == null || items.Count == 0)
                {
                    return new XResult<Int32>(0);
                }

                var tasks = new List<Task>(items.Count);
                var results = new ConcurrentQueue<Bill99AgreePayQueryResult>();
                StringBuilder sb = new StringBuilder();

                foreach (var item in items)
                {
                    //将3天前还没有结果的订单设置为失败
                    if ((DateTime.Now - item.CreateTime).TotalDays > 3)
                    {
                        sb.Append($"update pay_order set pay_status='{PayStatus.FAILURE.ToString()}', update_time='{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}' where out_trade_no='{item.OutTradeNo}';");
                    }

                    tasks.Add(Task.Run(() =>
                    {
                        QueryFromBill99(item.OutTradeNo, results);
                    }));
                }

                try
                {
                    Task.WaitAll(tasks.ToArray());
                }
                catch (Exception ex)
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, "Task.WaitAll(...)", "查询协议支付结果的并行任务出现异常", ex);
                }

                foreach (var result in results)
                {
                    if (result.PayStatus == PayStatus.SUCCESS
                        || result.PayStatus == PayStatus.FAILURE)
                    {
                        sb.Append($"update pay_order set pay_status='{result.PayStatus.ToString()}', update_time='{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}' where out_trade_no='{result.OutTradeNo}';");
                    }
                }

                if (sb.Length > 0)
                {
                    String sql = sb.ToString();
                    String traceMethod = $"{nameof(_payOrderRepository)}.ExecuteSql(...)";
                    _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.OK.ToString(), service, traceMethod, LogPhase.BEGIN, $"开始调用{traceMethod}", $"SQL：{sql}");
                    var execResult = _payOrderRepository.ExecuteSql(FormattableStringFactory.Create(sql));
                    _logger.Trace(TraceType.BLL.ToString(), (execResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), service, traceMethod, LogPhase.END, $"完成调用{traceMethod}", $"受影响{execResult.Value}行");
                    if (!execResult.Success)
                    {
                        _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, traceMethod, "更新协议支付结果失败", execResult.FirstException, $"SQL：{sql}");
                    }

                    return execResult;
                }

                return new XResult<Int32>(0);
            }
            finally
            {
                _lockProvider.UnLock(key);
            }
        }

        private void QueryFromBill99(String outTradeNo, ConcurrentQueue<Bill99AgreePayQueryResult> results)
        {
            if (String.IsNullOrWhiteSpace(outTradeNo))
            {
                return;
            }

            String service = $"{this.GetType().FullName}.QueryFromBill99(...)";

            var request = new AgreementQueryRequest()
            {
                Version = "1.0",
                QryTxnMsgContent = new QryTxnMsgRequestContent()
                {
                    ExternalRefNumber = outTradeNo,
                    TxnType = "PUR"
                }
            };

            String callMethod = $"{nameof(_api)}.AgreementQuery(...)";
            _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.OK.ToString(), service, callMethod, LogPhase.BEGIN, $"开始调用{callMethod}", new Object[] { ApiConfig.Bill99_AgreePay_Query_RequestUrl, request });

            var result = _api.AgreementQuery(ApiConfig.Bill99_AgreePay_Query_RequestUrl, request);

            _logger.Trace(TraceType.BLL.ToString(), (result.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), service, callMethod, LogPhase.BEGIN, $"完成调用{callMethod}", result.Value);

            if (!result.Success)
            {
                _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, callMethod, "查询快钱协议支付结果失败", result.FirstException, result.Value);
                return;
            }

            if (result.Value == null)
            {
                _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, callMethod, "未拉取到任何结果");
                return;
            }

            if (result.Value.ErrorMsgContent != null && result.Value.ErrorMsgContent.ErrorMessage.HasValue())
            {
                _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, callMethod, "查询快钱协议支付结果失败", result.FirstException, new
                {
                    OutTradeNo = outTradeNo,
                    result.Value.ErrorMsgContent
                });
                return;
            }

            if (result.Value.QryTxnMsgContent == null)
            {
                _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, callMethod, $"未拉取到任何结果，OutTradeNo={outTradeNo}");
                return;
            }

            if (result.Value.QryTxnMsgContent.ResponseCode != "00")
            {
                _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, callMethod, "查询快钱协议支付结果失败", result.FirstException, new
                {
                    OutTradeNo = outTradeNo,
                    result.Value.QryTxnMsgContent
                });
                return;
            }

            results.Enqueue(new Bill99AgreePayQueryResult()
            {
                Amount = result.Value.QryTxnMsgContent.Amount.ToDecimal(),
                OutTradeNo = result.Value.QryTxnMsgContent.ExternalRefNumber,
                PayStatus = CalcPayStatus(result.Value.QryTxnMsgContent.TxnStatus),
                CreateTime = result.Value.QryTxnMsgContent.EntryTime
            });
        }

        private PayStatus CalcPayStatus(String status)
        {
            switch (status)
            {
                case "S":
                    return PayStatus.SUCCESS;
                case "F":
                    return PayStatus.FAILURE;
            }

            return PayStatus.PROCESSING;
        }

        private String GetQueryStatusMsg(String status)
        {
            switch (status)
            {
                case "APPLY":
                    return PayStatus.APPLY.GetDescription();
                case "PROCESSING":
                    return PayStatus.PROCESSING.GetDescription();
                case "SUCCESS":
                    return $"支付{PayStatus.SUCCESS.GetDescription()}";
                case "FAILURE":
                    return $"支付{PayStatus.FAILURE.GetDescription()}";
            }

            return null;
        }
    }
}
