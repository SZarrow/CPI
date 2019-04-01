using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using CPI.Common;
using CPI.Common.Domain.EntrustPay;
using CPI.Common.Domain.SettleDomain.Bill99;
using CPI.Common.Domain.SettleDomain.Bill99.v1_0;
using CPI.Common.Exceptions;
using CPI.Common.Models;
using CPI.Config;
using CPI.IData.BaseRepositories;
using CPI.IService.EntrustPay;
using CPI.Providers;
using CPI.Utils;
using ATBase.Core;
using ATBase.Logging;
using ATBase.Payment.Bill99;
using ATBase.Payment.Bill99.Domain;

namespace CPI.Services.EntrustPay
{
    public class Bill99EntrustPaymentService : IEntrustPaymentService
    {
        private static readonly LockProvider _lockProvider = new LockProvider();
        private static readonly ILogger _logger = LogManager.GetLogger();
        private static readonly EntrustPaymentApi _api = CreateEntrustPaymentApi();

        private readonly IPayOrderRepository _payOrderRepository = null;
        private readonly IAllotAmountOrderRepository _allotAmountOrderRepository = null;

        private static EntrustPaymentApi CreateEntrustPaymentApi()
        {
            var factory = XDI.Resolve<IHttpClientFactory>();
            var client = factory.CreateClient("EntrustPaymentApiHttpClient");
            return new EntrustPaymentApi(client, GlobalConfig.X99bill_EntrustPay_Hehua_MerchantId, GlobalConfig.X99bill_EntrustPay_Hehua_TerminalId);
        }

        public XResult<CPIEntrustPayPaymentResponse> Pay(CPIEntrustPayPaymentRequest request)
        {
            if (request == null)
            {
                return new XResult<CPIEntrustPayPaymentResponse>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentNullException(nameof(request)));
            }

            if (!request.IsValid)
            {
                return new XResult<CPIEntrustPayPaymentResponse>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentException(request.ErrorMessage));
            }

            if (request.Amount < GlobalConfig.X99bill_EntrustPay_PayMinAmount)
            {
                return new XResult<CPIEntrustPayPaymentResponse>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentException($"支付金额至少为{GlobalConfig.X99bill_EntrustPay_PayMinAmount.ToString()}元"));
            }

            var parseSharingInfoResult = JsonUtil.DeserializeObject<SharingInfo>(request.SharingInfo);
            if (!parseSharingInfoResult.Success)
            {
                return new XResult<CPIEntrustPayPaymentResponse>(null, ErrorCode.DESERIALIZE_FAILED, new ArgumentException("解析SharingInfo参数失败"));
            }

            var sharingInfo = parseSharingInfoResult.Value;

            String service = $"{this.GetType().FullName}.Pay(...)";

            var requestHash = $"{request.OutTradeNo}".GetHashCode();

            if (_lockProvider.Exists(requestHash))
            {
                return new XResult<CPIEntrustPayPaymentResponse>(null, ErrorCode.SUBMIT_REPEAT);
            }

            try
            {
                if (!_lockProvider.Lock(requestHash))
                {
                    return new XResult<CPIEntrustPayPaymentResponse>(null, ErrorCode.SUBMIT_REPEAT);
                }

                var existsOutTradeNo = _payOrderRepository.Exists(x => x.OutTradeNo == request.OutTradeNo);
                if (existsOutTradeNo)
                {
                    return new XResult<CPIEntrustPayPaymentResponse>(null, ErrorCode.OUT_TRADE_NO_EXISTED);
                }

                //生成全局唯一的ID号
                String tradeNo = IDGenerator.GenerateID().ToString();

                // 1. 添加交易记录
                var newOrder = new PayOrder()
                {
                    Id = IDGenerator.GenerateID(),
                    AppId = request.AppId,
                    OutTradeNo = request.OutTradeNo,
                    TradeNo = tradeNo,
                    PayAmount = request.Amount,
                    BankCardNo = request.BankCardNo,
                    PayerId = request.PayerId,
                    PayChannelCode = GlobalConfig.X99BILL_PAYCHANNEL_CODE,
                    PayStatus = PayStatus.APPLY.ToString(),
                    PayType = PayType.ENTRUSTPAY.ToString(),
                    CreateTime = DateTime.Now
                };

                _payOrderRepository.Add(newOrder);
                var saveResult = _payOrderRepository.SaveChanges();
                if (!saveResult.Success)
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(_payOrderRepository)}.SaveChanges()", "支付单保存失败", saveResult.FirstException, newOrder);
                    return new XResult<CPIEntrustPayPaymentResponse>(null, ErrorCode.DB_UPDATE_FAILED, saveResult.FirstException);
                }

                //添加分账记录
                var allotAmountOrder = new AllotAmountOrder()
                {
                    Id = IDGenerator.GenerateID(),
                    AppId = request.AppId,
                    PayeeId = request.PayerId,
                    TradeNo = tradeNo,
                    OutTradeNo = request.OutTradeNo,
                    TotalAmount = request.Amount,
                    FeePayerId = sharingInfo.FeePayerId,
                    SharingType = sharingInfo.SharingType == "0" ? AllotAmountType.Pay.ToString() : AllotAmountType.Refund.ToString(),
                    SharingInfo = sharingInfo.SharingData,
                    ApplyTime = DateTime.Now,
                    Status = AllotAmountOrderStatus.APPLY.ToString()
                };

                _allotAmountOrderRepository.Add(allotAmountOrder);
                saveResult = _allotAmountOrderRepository.SaveChanges();

                if (!saveResult.Success)
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(_allotAmountOrderRepository)}.SaveChanges()", "分账数据保存失败", saveResult.FirstException, allotAmountOrder);
                    return new XResult<CPIEntrustPayPaymentResponse>(null, ErrorCode.DB_UPDATE_FAILED, saveResult.FirstException);
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

                var tradeTime = DateTime.Now;

                var payRequest = new EntrustPayRequest()
                {
                    Version = "1.0",
                    EntrustPayRequestContent = new EntrustPayRequestContent()
                    {
                        Amount = request.Amount.ToString(),
                        CardHolderId = request.IDCardNo,
                        CardHolderName = request.RealName,
                        CardNo = request.BankCardNo,
                        EntryTime = tradeTime.ToString("yyyyMMddHHmmss"),
                        ExternalRefNumber = request.OutTradeNo,
                        IdType = "0",
                        InteractiveStatus = "TR1",
                        TxnType = "PUR",
                        ExtMap = new ExtMap()
                        {
                            ExtDates = new ExtDate[] {
                                new ExtDate() { Key = "phone", Value = request.Mobile },
                                sharingExtDate
                            }
                        }
                    }
                };

                String callMethod = $"{_api.GetType().FullName}.EntrustPay(...)";

                _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.OK.ToString(), service, callMethod, LogPhase.BEGIN, $"开始调用{callMethod}", new Object[] { ApiConfig.Bill99_EntrustPay_Pay_RequestUrl, payRequest });

                var result = _api.EntrustPay(ApiConfig.Bill99_EntrustPay_Pay_RequestUrl, payRequest);

                _logger.Trace(TraceType.BLL.ToString(), (result.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), service, callMethod, LogPhase.END, $"完成调用{callMethod}", result.Value);

                if (!result.Success)
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, callMethod, "支付失败", result.FirstException, payRequest);
                    UpdateAllotAmountOrder(service, allotAmountOrder, AllotAmountOrderStatus.FAILURE);
                    return new XResult<CPIEntrustPayPaymentResponse>(null, ErrorCode.DEPENDENT_API_CALL_FAILED, result.FirstException);
                }

                if (result.Value == null || result.Value.EntrustPayResponseContent == null)
                {
                    _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, callMethod, LogPhase.ACTION, "快钱未返回任何数据");
                    UpdateAllotAmountOrder(service, allotAmountOrder, AllotAmountOrderStatus.FAILURE);
                    return new XResult<CPIEntrustPayPaymentResponse>(null, ErrorCode.REMOTE_RETURN_NOTHING);
                }

                var respContent = result.Value.EntrustPayResponseContent;
                if (respContent.ResponseCode != "00")
                {
                    UpdateAllotAmountOrder(service, allotAmountOrder, AllotAmountOrderStatus.FAILURE);
                    return new XResult<CPIEntrustPayPaymentResponse>(null, ErrorCode.DEPENDENT_API_CALL_FAILED, new RemoteException(respContent.ResponseTextMessage));
                }

                UpdateAllotAmountOrder(service, allotAmountOrder, AllotAmountOrderStatus.SUCCESS);

                var resp = new CPIEntrustPayPaymentResponse()
                {
                    OutTradeNo = respContent.ExternalRefNumber,
                    TradeNo = tradeNo,
                    PayTime = tradeTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    Status = PayStatus.SUCCESS.ToString(),
                    Msg = PayStatus.SUCCESS.GetDescription()
                };

                //根据返回码计算对应的支付状态，并更新到数据库
                var payStatus = Bill99Util.GetAgreepayPayStatus(respContent.ResponseCode);
                var payOrder = (from t0 in _payOrderRepository.QueryProvider
                                where t0.TradeNo == tradeNo
                                select t0).FirstOrDefault();

                //如果支付单存在，并且状态不是最终状态，才更新状态
                if (newOrder != null
                    && newOrder.PayStatus != PayStatus.SUCCESS.ToString()
                    && newOrder.PayStatus != PayStatus.FAILURE.ToString())
                {
                    newOrder.PayStatus = payStatus.ToString();
                    newOrder.UpdateTime = DateTime.Now;
                    _payOrderRepository.Update(newOrder);
                    saveResult = _payOrderRepository.SaveChanges();
                    if (!saveResult.Success)
                    {
                        _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(_payOrderRepository)}.SaveChanges()", "更新支付结果失败", saveResult.FirstException, newOrder);
                    }
                }

                return new XResult<CPIEntrustPayPaymentResponse>(resp);
            }
            finally
            {
                _lockProvider.UnLock(requestHash);
            }
        }

        private void UpdateAllotAmountOrder(String service, AllotAmountOrder allotAmountOrder, AllotAmountOrderStatus status)
        {
            allotAmountOrder.Status = status.ToString();
            _allotAmountOrderRepository.Update(allotAmountOrder);
            var saveResult = _allotAmountOrderRepository.SaveChanges();
            if (!saveResult.Success)
            {
                _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(_allotAmountOrderRepository)}.SaveChanges()", "更新分账状态失败", saveResult.FirstException, allotAmountOrder);
            }
        }

        public XResult<CPIEntrustPayQueryResponse> Query(CPIEntrustPayQueryRequest request)
        {
            return new XResult<CPIEntrustPayQueryResponse>(null, ErrorCode.METHOD_NOT_SUPPORT, new NotImplementedException());
        }
    }
}
