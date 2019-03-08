using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CPI.Common;
using CPI.Common.Domain.AgreePay;
using CPI.Common.Domain.AgreePay.YeePay;
using CPI.Common.Domain.Common;
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

                var execResult = YeePayAgreePayUtil.Execute<RawYeePayApplyBindCardRequest, RawYeePayApplyBindCardResponse>("/rest/v1.0/paperorder/unified/auth/request", new RawYeePayApplyBindCardRequest()
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

                String traceMethod = $"{nameof(YeePayAgreePayUtil)}.Execute(...)";

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

                var result = YeePayAgreePayUtil.Execute<RawYeePayBindCardRequest, RawYeePayBindCardResponse>("/rest/v1.0/paperorder/auth/confirm", new RawYeePayBindCardRequest()
                {
                    merchantno = GlobalConfig.YeePay_AgreePay_MerchantNo,
                    requestno = request.OutTradeNo,
                    validatecode = request.SmsValidCode
                });

                String callMethod = $"{nameof(YeePayAgreePayUtil)}.Execute(...)";

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

            String terminalPattern = $"^({Resources.YeePayAgreePayTerminalNo})|({Resources.YeePayEntrustPayTerminalNo})$";
            if (!Regex.IsMatch(request.TerminalNo, terminalPattern))
            {
                return new XResult<YeePayAgreePayPaymentResponse>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentException("TerminalNo字段传值错误"));
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
                    PayType = request.TerminalNo == Resources.YeePayAgreePayTerminalNo ? PayType.AGREEMENTPAY.ToString() : PayType.ENTRUSTPAY.ToString(),
                    CreateTime = tradeTime
                };

                _payOrderRepository.Add(newOrder);
                var saveResult = _payOrderRepository.SaveChanges();
                if (!saveResult.Success)
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(_payOrderRepository)}.SaveChanges()", "支付单保存失败", saveResult.FirstException, newOrder);
                    return new XResult<YeePayAgreePayPaymentResponse>(null, ErrorCode.DB_UPDATE_FAILED, saveResult.FirstException);
                }

                var result = YeePayAgreePayUtil.Execute<RawYeePayAgreePayPaymentRequest, RawYeePayAgreePayPaymentResponse>("/rest/v1.0/paperorder/unified/pay", new RawYeePayAgreePayPaymentRequest()
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

                String callMethod = $"{nameof(YeePayAgreePayUtil)}.Execute(...)";

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

                //如果易宝返回的不是PROCESSING则表示处理失败
                if (respResult.status != "PROCESSING")
                {
                    newOrder.PayStatus = PayStatus.FAILURE.ToString();
                    newOrder.UpdateTime = DateTime.Now;
                    UpdatePayOrder(service, newOrder);
                    return new XResult<YeePayAgreePayPaymentResponse>(null, ErrorCode.FAILURE, new RemoteException($"{respResult.errorcode}:{respResult.errormsg}"));
                }

                //如果易宝返回PROCESSING表示已处理
                //并且要将易宝返回的易宝内部交易号更新到数据库
                //这个TradeNo以后可以作为退款接口的原交易号
                newOrder.PayStatus = PayStatus.PROCESSING.ToString();
                newOrder.TradeNo = respResult.yborderid;
                newOrder.UpdateTime = DateTime.Now;
                UpdatePayOrder(service, newOrder);

                var resp = new YeePayAgreePayPaymentResponse()
                {
                    OutTradeNo = respResult.requestno,
                    YeePayTradeNo = respResult.yborderid,
                    ApplyTime = tradeTime.ToString("yyyy-MM-dd HH:mm:ss"),
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

        public XResult<YeePayAgreePayRefundResponse> Refund(YeePayAgreePayRefundRequest request)
        {
            if (request == null)
            {
                return new XResult<YeePayAgreePayRefundResponse>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentNullException(nameof(request)));
            }

            if (!request.IsValid)
            {
                return new XResult<YeePayAgreePayRefundResponse>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentException(request.ErrorMessage));
            }

            String service = $"{this.GetType().FullName}.Refund(...)";

            var requestHash = $"refund:{request.OutTradeNo}".GetHashCode();

            if (_lockProvider.Exists(requestHash))
            {
                return new XResult<YeePayAgreePayRefundResponse>(null, ErrorCode.SUBMIT_REPEAT);
            }

            try
            {
                if (!_lockProvider.Lock(requestHash))
                {
                    return new XResult<YeePayAgreePayRefundResponse>(null, ErrorCode.SUBMIT_REPEAT);
                }

                // 保证外部交易号不重复
                var existsOutTradeNo = _payOrderRepository.Exists(x => x.AppId == request.AppId && x.OutTradeNo == request.OutTradeNo);
                if (existsOutTradeNo)
                {
                    return new XResult<YeePayAgreePayRefundResponse>(null, ErrorCode.OUT_TRADE_NO_EXISTED);
                }

                //保证原始单号是存在的
                var originalPayOrder = _payOrderRepository.QueryProvider.FirstOrDefault(x => x.OutTradeNo == request.OriginalOutTradeNo);
                if (originalPayOrder == null)
                {
                    return new XResult<YeePayAgreePayRefundResponse>(null, ErrorCode.INFO_NOT_EXIST, new ArgumentException("原支付单不存在"));
                }

                if (request.Amount < 0.01m || request.Amount > originalPayOrder.PayAmount)
                {
                    return new XResult<YeePayAgreePayRefundResponse>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentException("退款金额必须大于0.01元且小于或等于支付金额"));
                }

                //生成全局唯一的ID号
                Int64 newId = IDGenerator.GenerateID();
                var tradeTime = DateTime.Now;

                // 添加退款记录
                var refundOrder = new PayOrder()
                {
                    Id = newId,
                    AppId = request.AppId,
                    PayerId = originalPayOrder.PayerId,
                    OutTradeNo = request.OutTradeNo,
                    TradeNo = originalPayOrder.TradeNo,
                    PayAmount = request.Amount,
                    BankCardNo = originalPayOrder.BankCardNo,
                    PayChannelCode = GlobalConfig.YEEPAY_PAYCHANNEL_CODE,
                    PayStatus = PayStatus.APPLY.ToString(),
                    PayType = PayType.REFUND.ToString(),
                    CreateTime = tradeTime,
                    Remark = request.Remark
                };

                _payOrderRepository.Add(refundOrder);
                var saveResult = _payOrderRepository.SaveChanges();
                if (!saveResult.Success)
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(_payOrderRepository)}.SaveChanges()", "支付单保存失败", saveResult.FirstException, refundOrder);
                    return new XResult<YeePayAgreePayRefundResponse>(null, ErrorCode.DB_UPDATE_FAILED, saveResult.FirstException);
                }

                var execResult = YeePayAgreePayUtil.Execute<RawYeePayRefundRequest, RawYeePayRefundResponse>("/rest/v1.0/paperorder/api/refund/request", new RawYeePayRefundRequest()
                {
                    merchantno = GlobalConfig.YeePay_AgreePay_MerchantNo,
                    requestno = request.OutTradeNo,
                    paymentyborderid = refundOrder.TradeNo,
                    amount = request.Amount.ToString("0.00"),
                    requesttime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    remark = request.Remark
                });

                String callMethod = $"{nameof(YeePayAgreePayUtil)}.Execute(...)";

                if (!execResult.Success)
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, callMethod, "支付失败", execResult.FirstException, execResult);
                    return new XResult<YeePayAgreePayRefundResponse>(null, ErrorCode.DEPENDENT_API_CALL_FAILED, execResult.FirstException);
                }

                if (execResult.Value == null)
                {
                    return new XResult<YeePayAgreePayRefundResponse>(null, ErrorCode.REMOTE_RETURN_NOTHING);
                }

                var respResult = execResult.Value;

                //如果易宝返回的不是PROCESSING则表示处理失败
                if (respResult.status != "PROCESSING")
                {
                    refundOrder.PayStatus = PayStatus.FAILURE.ToString();
                    refundOrder.UpdateTime = DateTime.Now;
                    UpdatePayOrder(service, refundOrder);
                    return new XResult<YeePayAgreePayRefundResponse>(null, ErrorCode.FAILURE, new RemoteException($"{respResult.errorcode}:{respResult.errormsg}"));
                }

                //如果易宝返回PROCESSING表示已处理
                //并且要将易宝返回的易宝内部交易号更新到数据库
                //这个TradeNo以后可以作为退款接口的原交易号
                refundOrder.PayStatus = PayStatus.PROCESSING.ToString();
                refundOrder.TradeNo = respResult.yborderid;
                refundOrder.UpdateTime = DateTime.Now;
                UpdatePayOrder(service, refundOrder);

                var resp = new YeePayAgreePayRefundResponse()
                {
                    OutTradeNo = respResult.requestno,
                    YeePayTradeNo = respResult.yborderid,
                    ApplyTime = tradeTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    Status = PayStatus.PROCESSING.ToString(),
                    Msg = PayStatus.PROCESSING.GetDescription()
                };

                return new XResult<YeePayAgreePayRefundResponse>(resp);
            }
            finally
            {
                _lockProvider.UnLock(requestHash);
            }
        }

        private void UpdatePayOrder(String service, PayOrder order)
        {
            _payOrderRepository.Update(order);
            var saveResult = _payOrderRepository.SaveChanges();
            if (!saveResult.Success)
            {
                _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(_payOrderRepository)}.SaveChanges()", "更新协议支付结果失败", saveResult.FirstException, order);
            }
        }

        public XResult<Int32> PullPayStatus(Int32 count)
        {
            if (count <= 0)
            {
                return new XResult<Int32>(0, ErrorCode.INVALID_ARGUMENT, new ArgumentOutOfRangeException($"参数count必须大于0"));
            }

            String service = $"{this.GetType().FullName}.PullPayStatus(...)";

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
                             && t0.PayChannelCode == GlobalConfig.YEEPAY_PAYCHANNEL_CODE
                             && (t0.PayType == PayType.AGREEMENTPAY.ToString() || t0.PayType == PayType.ENTRUSTPAY.ToString())
                             orderby t0.CreateTime
                             select new PullQueryItem(t0.OutTradeNo, t0.CreateTime)).Take(count).ToList();
                }
                catch (Exception ex)
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, "items", "查询易宝协议支付/代扣处理中的订单失败", ex);
                    return new XResult<Int32>(0);
                }

                if (items == null || items.Count == 0)
                {
                    return new XResult<Int32>(0);
                }

                var tasks = new List<Task>(items.Count);
                var results = new ConcurrentQueue<YeePayAgreePayQueryResult>();
                StringBuilder sb = new StringBuilder();

                foreach (var item in items)
                {
                    tasks.Add(Task.Run(() =>
                    {
                        QueryPayResultFromYeePay(item.OutTradeNo, results);
                    }));
                }

                try
                {
                    Task.WaitAll(tasks.ToArray());
                }
                catch (Exception ex)
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, "Task.WaitAll(...)", "查询易宝协议支付/代扣结果的并行任务出现异常", ex);
                }

                foreach (var result in results)
                {
                    if (result.PayStatus == PayStatus.SUCCESS
                        || result.PayStatus == PayStatus.FAILURE)
                    {
                        sb.Append($"update pay_order set pay_status='{result.PayStatus.ToString()}', update_time='{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}' where out_trade_no='{result.OutTradeNo}' and pay_channel_code='{GlobalConfig.YEEPAY_PAYCHANNEL_CODE}';");
                    }
                }

                if (sb.Length > 0)
                {
                    String sql = sb.ToString();
                    String traceMethod = $"{nameof(_payOrderRepository)}.ExecuteSql(...)";
                    var execResult = _payOrderRepository.ExecuteSql(FormattableStringFactory.Create(sql));
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

        public XResult<Int32> PullRefundStatus(Int32 count)
        {
            if (count <= 0)
            {
                return new XResult<Int32>(0, ErrorCode.INVALID_ARGUMENT, new ArgumentOutOfRangeException($"参数count必须大于0"));
            }

            String service = $"{this.GetType().FullName}.PullRefundStatus(...)";

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
                             && t0.PayChannelCode == GlobalConfig.YEEPAY_PAYCHANNEL_CODE
                             && t0.PayType == PayType.REFUND.ToString()
                             orderby t0.CreateTime
                             select new PullQueryItem(t0.OutTradeNo, t0.CreateTime)).Take(count).ToList();
                }
                catch (Exception ex)
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, "items", "查询退款处理中的订单失败", ex);
                    return new XResult<Int32>(0);
                }

                if (items == null || items.Count == 0)
                {
                    return new XResult<Int32>(0);
                }

                var tasks = new List<Task>(items.Count);
                var results = new ConcurrentQueue<YeePayAgreePayQueryResult>();
                StringBuilder sb = new StringBuilder();

                foreach (var item in items)
                {
                    tasks.Add(Task.Run(() =>
                    {
                        QueryRefundResultFromYeePay(item.OutTradeNo, results);
                    }));
                }

                try
                {
                    Task.WaitAll(tasks.ToArray());
                }
                catch (Exception ex)
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, "Task.WaitAll(...)", "查询退款结果的并行任务出现异常", ex);
                }

                foreach (var result in results)
                {
                    if (result.PayStatus == PayStatus.SUCCESS
                        || result.PayStatus == PayStatus.FAILURE)
                    {
                        sb.Append($"update pay_order set pay_status='{result.PayStatus.ToString()}', update_time='{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}' where out_trade_no='{result.OutTradeNo}' and pay_channel_code='{GlobalConfig.YEEPAY_PAYCHANNEL_CODE}';");
                    }
                }

                if (sb.Length > 0)
                {
                    String sql = sb.ToString();
                    String traceMethod = $"{nameof(_payOrderRepository)}.ExecuteSql(...)";
                    var execResult = _payOrderRepository.ExecuteSql(FormattableStringFactory.Create(sql));
                    if (!execResult.Success)
                    {
                        _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, traceMethod, "更新退款结果失败", execResult.FirstException, $"SQL：{sql}");
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

        private void QueryPayResultFromYeePay(String outTradeNo, ConcurrentQueue<YeePayAgreePayQueryResult> results)
        {
            var execResult = YeePayAgreePayUtil.Execute<Object, RawYeePayAgreePayResultQueryResponse>("/rest/v1.0/paperorder/api/pay/query", new
            {
                merchantno = GlobalConfig.YeePay_AgreePay_MerchantNo,
                requestno = outTradeNo
            });

            if (execResult.Success && execResult.Value != null)
            {
                var respResult = execResult.Value;
                if (respResult.status == "PAY_SUCCESS" || respResult.status == "PAY_FAIL" || respResult.status == String.Empty)
                {
                    var result = new YeePayAgreePayQueryResult()
                    {
                        Amount = respResult.amount,
                        CompleteTime = respResult.banksuccessdate,
                        OutTradeNo = respResult.requestno,
                        YeePayTradeNo = respResult.yborderid,
                        PayStatus = respResult.status == "PAY_SUCCESS" ? PayStatus.SUCCESS : PayStatus.FAILURE
                    };
                    results.Enqueue(result);
                }
            }
        }

        private void QueryRefundResultFromYeePay(String outTradeNo, ConcurrentQueue<YeePayAgreePayQueryResult> results)
        {
            var execResult = YeePayAgreePayUtil.Execute<Object, RawYeePayRefundResultQueryResponse>("/rest/v1.0/paperorder/api/refund/query", new
            {
                merchantno = GlobalConfig.YeePay_AgreePay_MerchantNo,
                requestno = outTradeNo
            });

            if (execResult.Success && execResult.Value != null)
            {
                var respResult = execResult.Value;
                var result = new YeePayAgreePayQueryResult()
                {
                    Amount = respResult.amount,
                    CompleteTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    OutTradeNo = respResult.requestno,
                    YeePayTradeNo = respResult.yborderid
                };

                Boolean changed = false;
                switch (respResult.status)
                {
                    case "REFUND_SUCCESS":
                        result.PayStatus = PayStatus.SUCCESS;
                        changed = true;
                        break;
                    case "":
                    case "REFUND_FAIL":
                        result.PayStatus = PayStatus.FAILURE;
                        changed = true;
                        break;
                }

                if (changed)
                {
                    results.Enqueue(result);
                }
            }
        }
    }
}
