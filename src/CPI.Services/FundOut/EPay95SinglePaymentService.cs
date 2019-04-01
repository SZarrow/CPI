using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using CPI.Common;
using CPI.Common.Domain.Common;
using CPI.Common.Domain.FundOut.EPay95;
using CPI.Common.Exceptions;
using CPI.Common.Models;
using CPI.Config;
using CPI.IData.BaseRepositories;
using CPI.IService.FundOut;
using CPI.Providers;
using CPI.Utils;
using ATBase.Core;
using ATBase.Core.Collections;
using ATBase.Logging;
using ATBase.Net;
using ATBase.Security;

namespace CPI.Services.FundOut
{
    public class EPay95SinglePaymentService : IEPay95SinglePaymentService
    {
        private static readonly ILogger _logger = LogManager.GetLogger();
        private static readonly LockProvider _lockProvider = new LockProvider();
        private static readonly IHttpClientFactory _httpClientFactory = XDI.Resolve<IHttpClientFactory>();

        private readonly IFundOutOrderRepository _fundOutOrderRepository = null;

        public XResult<PayResponse> Pay(PayRequest request)
        {
            if (request == null)
            {
                return new XResult<PayResponse>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentNullException(nameof(request)));
            }

            String service = $"{this.GetType().FullName}.Pay(...)";

            if (!request.IsValid)
            {
                return new XResult<PayResponse>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentException(request.ErrorMessage));
            }

            var requestHash = $"pay:{request.AppId}.{request.OutTradeNo}".GetHashCode();

            if (_lockProvider.Exists(requestHash))
            {
                return new XResult<PayResponse>(null, ErrorCode.SUBMIT_REPEAT);
            }

            try
            {
                if (!_lockProvider.Lock(requestHash))
                {
                    return new XResult<PayResponse>(null, ErrorCode.SUBMIT_REPEAT);
                }

                var existsOutTradeNo = _fundOutOrderRepository.Exists(x => x.OutTradeNo == request.OutTradeNo);
                if (existsOutTradeNo)
                {
                    return new XResult<PayResponse>(null, ErrorCode.OUT_TRADE_NO_EXISTED);
                }

                //申请冻结商户放款余额
                var applyFreezeLoanParas = new Dictionary<String, String>(4);
                applyFreezeLoanParas["MerchantNo"] = request.MerchantNo;
                applyFreezeLoanParas["OrderNo"] = request.OutTradeNo;
                applyFreezeLoanParas["FrozenLoanBalance"] = request.Amount;
                applyFreezeLoanParas["Sign"] = MerchantUtil.MD5Sign(applyFreezeLoanParas);

                var client = GetClient();

                String traceMethod = $"{nameof(client)}.PostForm(...)";

                _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.OK.ToString(), service, traceMethod, LogPhase.BEGIN, "开始请求商户系统冻结放款余额", new Object[] { ApiConfig.SystemMerchantAccountBalanceFreezeRequestUrl, applyFreezeLoanParas });

                var freezeResult = client.PostForm<ApiResult>(ApiConfig.SystemMerchantAccountBalanceFreezeRequestUrl, applyFreezeLoanParas);

                _logger.Trace(TraceType.BLL.ToString(), (freezeResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), service, traceMethod, LogPhase.END, "结束请求商户系统冻结放款余额", freezeResult.Value);

                if (!freezeResult.Success)
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, traceMethod, "商户系统冻结放款余额失败", freezeResult.FirstException, applyFreezeLoanParas);
                    return new XResult<PayResponse>(null, ErrorCode.DEPENDENT_API_CALL_FAILED, new RemoteException("商户系统冻结放款余额失败"));
                }

                if (freezeResult.Value == null)
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, traceMethod, "商户系统冻结放款余额返回的业务数据为空", null, applyFreezeLoanParas);
                    return new XResult<PayResponse>(null, ErrorCode.DEPENDENT_API_CALL_FAILED, new RemoteException("商户系统冻结放款余额返回的业务数据为空"));
                }

                if (freezeResult.Value.Status != "SUCCESS")
                {
                    _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, traceMethod, LogPhase.ACTION, "商户系统冻结放款余额失败", freezeResult.Value);
                    return new XResult<PayResponse>(null, ErrorCode.DEPENDENT_API_CALL_FAILED, new RemoteException("商户系统冻结放款余额失败"));
                }

                var now = DateTime.Now;
                var newId = IDGenerator.GenerateID();

                var fundoutOrder = new FundOutOrder()
                {
                    Id = newId,
                    TradeNo = newId.ToString(),
                    AppId = request.AppId,
                    OutTradeNo = request.OutTradeNo,
                    Amount = request.Amount.ToDecimal(),
                    RealName = request.RealName,
                    BankCardNo = request.BankCardNo,
                    Mobile = request.Mobile,
                    Remark = request.Remark,
                    PayStatus = PayStatus.APPLY.ToString(),
                    CreateTime = DateTime.Now
                };

                _fundOutOrderRepository.Add(fundoutOrder);
                var saveResult = _fundOutOrderRepository.SaveChanges();
                if (!saveResult.Success)
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(_fundOutOrderRepository)}.SaveChanges()", "保存代付订单数据失败", saveResult.FirstException, fundoutOrder);
                    return new XResult<PayResponse>(null, ErrorCode.DB_UPDATE_FAILED, new DbUpdateException("保存订单数据失败"));
                }

                var dic = new Dictionary<String, String>(6);
                dic["LoanJsonList"] = JsonUtil.SerializeObject(new FundOutLoanInfo()
                {
                    Amount = request.Amount,
                    CardNumber = CryptoHelper.RSAEncrypt(request.BankCardNo, KeyConfig.EPay95_FundOut_PublicKey).Value,
                    IdentificationNo = request.IDCardNo,
                    Mobile = request.Mobile,
                    OrderNo = request.OutTradeNo,
                    RealName = request.RealName,
                    Type = "0"
                }).Value;
                dic["PlatformMoneymoremore"] = GlobalConfig.X95epay_FundOut_Hehua_PlatformMoneymoremore;
                dic["BatchNo"] = request.OutTradeNo;
                dic["Remark"] = request.Remark;
                dic["NotifyURL"] = ApiConfig.EPay95_FundOut_Pay_NotifyUrl;

                var signResult = EPay95Util.MakeSign(dic);
                if (signResult.IsNullOrWhiteSpace())
                {
                    return new XResult<PayResponse>(null, ErrorCode.SIGN_FAILED);
                }

                dic["LoanJsonList"] = HttpUtility.UrlEncode(dic["LoanJsonList"]);
                dic["SignInfo"] = signResult;

                _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.OK.ToString(), service, traceMethod, LogPhase.BEGIN, $"开始调用{ApiConfig.EPay95_FundOut_Pay_RequestUrl}", new Object[] { ApiConfig.EPay95_FundOut_Pay_RequestUrl, dic });

                //记录请求开始时间
                fundoutOrder.ApplyTime = DateTime.Now;
                var respMsgResult = client.PostForm(ApiConfig.EPay95_FundOut_Pay_RequestUrl, dic);

                _logger.Trace(TraceType.BLL.ToString(), (respMsgResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), service, traceMethod, LogPhase.ACTION, $"结束调用{ApiConfig.EPay95_FundOut_Pay_RequestUrl}");

                if (!respMsgResult.Success)
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, traceMethod, "代付失败", respMsgResult.FirstException, dic);
                    return new XResult<PayResponse>(null, ErrorCode.DEPENDENT_API_CALL_FAILED, new RequestException(respMsgResult.ErrorMessage));
                }

                String respContent = null;
                try
                {
                    respContent = respMsgResult.Value.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.OK.ToString(), service, traceMethod, LogPhase.END, $"调用{ApiConfig.EPay95_FundOut_Pay_RequestUrl}返回结果", respContent);
                }
                catch (Exception ex)
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, traceMethod, "读取代付返回的消息内容出现异常", ex, dic);
                    return new XResult<PayResponse>(null, ErrorCode.RESPONSE_READ_FAILED, ex);
                }

                if (respContent.IsNullOrWhiteSpace())
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, traceMethod, "双乾未返回任何数据", null, dic);
                    return new XResult<PayResponse>(null, ErrorCode.REMOTE_RETURN_NOTHING);
                }

                EPay95PayReturnResult respResult = JsonUtil.DeserializeObject<EPay95PayReturnResult>(respContent).Value;
                if (respResult == null)
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, "respResult", "无法将代付结果反序列化为EPay95PayReturnResult对象", null, respContent);
                    return new XResult<PayResponse>(null, ErrorCode.DESERIALIZE_FAILED);
                }

                //记录请求结束时间
                fundoutOrder.EndTime = DateTime.Now;

                switch (respResult.ResultCode)
                {
                    case "88":
                    case "90":
                    case "15":
                        //修改支付状态为正在处理中
                        fundoutOrder.PayStatus = PayStatus.PROCESSING.ToString();
                        break;
                    default:
                        //修改支付状态为正在处理中
                        fundoutOrder.PayStatus = PayStatus.FAILURE.ToString();
                        break;
                }

                fundoutOrder.UpdateTime = DateTime.Now;
                _fundOutOrderRepository.Update(fundoutOrder);
                var updateResult = _fundOutOrderRepository.SaveChanges();
                if (!updateResult.Success)
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(_fundOutOrderRepository)}.SaveChanges()", "更新支付状态失败", updateResult.FirstException, fundoutOrder);
                }

                var payResp = new PayResponse()
                {
                    Status = fundoutOrder.PayStatus,
                    Msg = respResult.Message.HasValue() ? respResult.Message : PayStatus.PROCESSING.GetDescription()
                };

                if (respResult.LoanJsonList != null)
                {
                    payResp.Amount = respResult.LoanJsonList.Amount;
                    payResp.BankCardNo = respResult.LoanJsonList.CardNumber;
                    payResp.OutTradeNo = respResult.LoanJsonList.OrderNo;
                }

                return new XResult<PayResponse>(payResp);
            }
            finally
            {
                _lockProvider.UnLock(requestHash);
            }
        }

        public XResult<PagedList<QueryDetailResult>> QueryDetails(QueryRequest request)
        {
            if (request == null)
            {
                return new XResult<PagedList<QueryDetailResult>>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentNullException(nameof(request)));
            }

            if (!request.IsValid)
            {
                return new XResult<PagedList<QueryDetailResult>>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentException(request.ErrorMessage));
            }

            var q = _fundOutOrderRepository.QueryProvider;//.Where(x => x.AppId == request.AppId);

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
                var ds = q.Select(x => new QueryDetailResult()
                {
                    OutTradeNo = x.OutTradeNo,
                    Status = x.PayStatus,
                    Msg = GetQueryStatusMsg(x.PayStatus),
                    Amount = x.Amount,
                    BankCardNo = x.BankCardNo,
                    RealName = x.RealName,
                    CreateTime = x.CreateTime
                }).OrderByDescending(x => x.CreateTime);
                var result = new PagedList<QueryDetailResult>(ds, request.PageIndex, request.PageSize);
                return new XResult<PagedList<QueryDetailResult>>(result);
            }
            catch (Exception ex)
            {
                return new XResult<PagedList<QueryDetailResult>>(null, ErrorCode.DB_QUERY_FAILED, ex);
            }
        }

        public XResult<PagedList<QueryStatusResult>> QueryStatus(QueryRequest request)
        {
            if (request == null)
            {
                return new XResult<PagedList<QueryStatusResult>>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentNullException(nameof(request)));
            }

            if (!request.IsValid)
            {
                return new XResult<PagedList<QueryStatusResult>>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentException(request.ErrorMessage));
            }

            var q = _fundOutOrderRepository.QueryProvider;//TODO:.Where(x => x.AppId == request.AppId);

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
                var ds = q.Select(x => new QueryStatusResult()
                {
                    OutTradeNo = x.OutTradeNo,
                    Status = x.PayStatus,
                    Msg = GetQueryStatusMsg(x.PayStatus),
                    Amount = x.Amount,
                    BankCardNo = x.BankCardNo,
                    CreateTime = x.CreateTime
                }).OrderByDescending(x => x.CreateTime);
                var result = new PagedList<QueryStatusResult>(ds, request.PageIndex, request.PageSize);
                return new XResult<PagedList<QueryStatusResult>>(result);
            }
            catch (Exception ex)
            {
                return new XResult<PagedList<QueryStatusResult>>(null, ErrorCode.DB_QUERY_FAILED, ex);
            }
        }

        public XResult<Boolean> UpdatePayStatus(PayNotifyResult result)
        {
            if (result == null)
            {
                return new XResult<Boolean>(false, ErrorCode.INVALID_ARGUMENT, new ArgumentNullException(nameof(result)));
            }

            String service = $"{this.GetType().FullName}.UpdatePayStatus(...)";

            _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.OK.ToString(), service, ":", LogPhase.BEGIN, "开始更新支付通知结果", result);

            if (!result.IsValid)
            {
                _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(result)}.IsValid", $"支付结果对象验证失败：{result.ErrorMessage}", null, result);
                return new XResult<Boolean>(false, ErrorCode.INVALID_ARGUMENT, new ArgumentException(result.ErrorMessage));
            }

            var requestHash = $"UpdatePayStatus:{result.BatchNo}".GetHashCode();

            if (_lockProvider.Exists(requestHash))
            {
                return new XResult<Boolean>(false, ErrorCode.SUBMIT_REPEAT);
            }

            try
            {
                if (!_lockProvider.Lock(requestHash))
                {
                    return new XResult<Boolean>(false, ErrorCode.SUBMIT_REPEAT);
                }

                var existedOrder = _fundOutOrderRepository.QueryProvider.Where(x => x.OutTradeNo == result.BatchNo).FirstOrDefault();
                if (existedOrder == null)
                {
                    _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.OK.ToString(), service, "existedOrder", LogPhase.ACTION, $"更新的订单不存在，订单编号：{result.BatchNo}");
                    return new XResult<Boolean>(false, ErrorCode.OUT_TRADE_NO_NOT_EXIST);
                }

                if (existedOrder.PayStatus != PayStatus.FAILURE.ToString()
                    && existedOrder.PayStatus != PayStatus.SUCCESS.ToString())
                {
                    Boolean statusHasChanged = false;
                    switch (result.ResultCode)
                    {
                        case "88":
                            existedOrder.PayStatus = PayStatus.SUCCESS.ToString();
                            existedOrder.UpdateTime = DateTime.Now;
                            statusHasChanged = true;
                            break;
                        case "15":
                        case "90":
                            break;
                        default:
                            existedOrder.PayStatus = PayStatus.FAILURE.ToString();
                            existedOrder.UpdateTime = DateTime.Now;
                            statusHasChanged = true;
                            break;
                    }

                    if (statusHasChanged)
                    {
                        _fundOutOrderRepository.Update(existedOrder);
                        var updateResult = _fundOutOrderRepository.SaveChanges();
                        if (!updateResult.Success)
                        {
                            _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(_fundOutOrderRepository)}.SaveChanges()", "更新代付结果状态失败", updateResult.FirstException, existedOrder);
                            return new XResult<Boolean>(false, updateResult.FirstException);
                        }
                    }
                }

                _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.OK.ToString(), service, ":", LogPhase.END, "结束更新支付通知结果");

                return new XResult<Boolean>(true);
            }
            finally
            {
                _lockProvider.UnLock(requestHash);
            }
        }

        public XResult<Boolean> PullPayResult()
        {
            throw new NotImplementedException();
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

        private static HttpClient GetClient()
        {
            return _httpClientFactory.CreateClient("CommonHttpClient");
        }
    }
}
