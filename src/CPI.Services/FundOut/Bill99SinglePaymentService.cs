using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Linq;
using CPI.Common;
using CPI.Common.Domain.Common;
using CPI.Common.Domain.FundOut.Bill99;
using CPI.Common.Exceptions;
using CPI.Common.Models;
using CPI.Config;
using CPI.IData.BaseRepositories;
using CPI.IService.FundOut;
using CPI.Providers;
using CPI.Security;
using CPI.Utils;
using Lotus.Core;
using Lotus.Core.Collections;
using Lotus.Logging;
using Lotus.Net;
using Lotus.Security;
using Lotus.Serialization;

namespace CPI.Services.FundOut
{
    public class Bill99SinglePaymentService : IBill99SinglePaymentService
    {
        private static readonly ILogger _logger = LogManager.GetLogger();
        private static readonly HttpClient _client = CreateHttpClient();
        private static readonly LockProvider _lockProvider = new LockProvider();

        private readonly XSerializer _serializer = new XSerializer();
        private readonly IFundOutOrderRepository _fundOutOrderRepository = null;

        public XResult<SingleSettlementPaymentApplyResponse> Pay(SingleSettlementPaymentApplyRequest request)
        {
            if (request == null)
            {
                return new XResult<SingleSettlementPaymentApplyResponse>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentNullException($"{nameof(request)}为null"));
            }

            if (!request.IsValid)
            {
                return new XResult<SingleSettlementPaymentApplyResponse>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentException(request.ErrorMessage));
            }

            String service = $"{this.GetType().FullName}.Pay(...)";

            var requestHash = $"payorder:{request.OrderNo}".GetHashCode();

            if (_lockProvider.Exists(requestHash))
            {
                return new XResult<SingleSettlementPaymentApplyResponse>(null, ErrorCode.SUBMIT_REPEAT);
            }

            try
            {
                if (!_lockProvider.Lock(requestHash))
                {
                    return new XResult<SingleSettlementPaymentApplyResponse>(null, ErrorCode.SUBMIT_REPEAT);
                }

                Boolean existOrder = true;
                try
                {
                    existOrder = (from t0 in _fundOutOrderRepository.QueryProvider
                                  where t0.OutTradeNo == request.OrderNo
                                  select t0.Id).Count() > 0;
                }
                catch (Exception ex)
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, "existOrder", "查询订单失败", ex, request);
                    return new XResult<SingleSettlementPaymentApplyResponse>(null, ErrorCode.DB_QUERY_FAILED, new DbQueryException($"查询订单号失败，订单号：{request.OrderNo}"));
                }

                if (existOrder)
                {
                    return new XResult<SingleSettlementPaymentApplyResponse>(null, ErrorCode.OUT_TRADE_NO_EXISTED);
                }

                Int64 newId = IDGenerator.GenerateID();
                String tradeNo = newId.ToString();

                var fundoutOrder = new FundOutOrder()
                {
                    Id = newId,
                    AppId = request.AppId,
                    TradeNo = tradeNo,
                    OutTradeNo = request.OrderNo,
                    Amount = request.Amount,
                    RealName = request.CreditName,
                    PayStatus = PayStatus.PROCESSING.ToString(),
                    FeeAction = request.FeeAction,
                    BankCardNo = request.BankCardNo,
                    BankName = request.BankName,
                    Mobile = request.Mobile,
                    CreateTime = DateTime.Now,
                    Remark = request.Remark
                };

                //入库
                _fundOutOrderRepository.Add(fundoutOrder);
                var saveResult = _fundOutOrderRepository.SaveChanges();
                if (!saveResult.Success)
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(_fundOutOrderRepository)}.SaveChanges()", "保存订单失败", saveResult.FirstException, fundoutOrder);
                    return new XResult<SingleSettlementPaymentApplyResponse>(null, ErrorCode.DB_UPDATE_FAILED, new DbUpdateException($"保存订单失败，订单编号：{fundoutOrder.OutTradeNo}"));
                }

                //开始执行代付
                var requestBuilder = new RequestBuilder(_serializer, _logger)
                {
                    EncryptKey = CryptoHelper.GenerateRandomKey(),
                    RequestType = "pay2BankRequest",
                    RequestHead = "pay2bankHead",
                    RequestBodyType = "requestBody",
                    RequestBody = request
                };

                var postXml = requestBuilder.Build(doc =>
                {
                    var amountEl = doc.Root.Element("amount");
                    if (amountEl != null)
                    {
                        var amountText = amountEl.Value;
                        if (Decimal.TryParse(amountText, out Decimal amount))
                        {
                            amountEl.Value = Convert.ToInt32(amount * 100m).ToString();
                        }
                    }
                });

                //更新申请时间
                fundoutOrder.ApplyTime = DateTime.Now;

                String callMethod = $"{nameof(_client)}.PostXml(...)";

                _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.OK.ToString(), service, callMethod, LogPhase.BEGIN, $"开始调用{callMethod}", new Object[] { ApiConfig.Bill99FOSinglePayApplyRequestUrl, postXml });

                var postResult = _client.PostXml(ApiConfig.Bill99FOSinglePayApplyRequestUrl, postXml);

                _logger.Trace(TraceType.BLL.ToString(), (postResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), service, callMethod, LogPhase.ACTION, $"完成调用{callMethod}");

                if (!postResult.Success)
                {
                    return new XResult<SingleSettlementPaymentApplyResponse>(null, ErrorCode.DEPENDENT_API_CALL_FAILED, postResult.FirstException);
                }

                String resp = null;
                try
                {
                    resp = postResult.Value.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.OK.ToString(), service, callMethod, LogPhase.END, $"调用{callMethod}结果", resp);
                }
                catch (Exception ex)
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, callMethod, "读取第三方返回的消息内容失败", ex, postXml);
                    return new XResult<SingleSettlementPaymentApplyResponse>(null, ErrorCode.DEPENDENT_API_CALL_FAILED, ex);
                }

                if (String.IsNullOrWhiteSpace(resp))
                {
                    return new XResult<SingleSettlementPaymentApplyResponse>(null, ErrorCode.REMOTE_RETURN_NOTHING);
                }

                //更新结束时间
                fundoutOrder.EndTime = DateTime.Now;

                var parseResult = ParseXml(resp);
                if (!parseResult.Success)
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, "ParseXml(...)", "解析返回数据失败", parseResult.FirstException, resp);
                    return new XResult<SingleSettlementPaymentApplyResponse>(null, ErrorCode.XML_PARSE_FAILED, new SystemException("解析返回数据失败"));
                }

                var root = parseResult.Value.Root;

                var memberCodeEl = root.Descendants("memberCode").FirstOrDefault();
                if (memberCodeEl == null)
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, "memberCodeEl", "返回数据中未包含<memberCode></memberCode>元素");
                    return new XResult<SingleSettlementPaymentApplyResponse>(null, ErrorCode.XML_ELEMENT_NOT_EXIST, new RemoteException("<memberCode> not found"));
                }

                var decodeResult = Decode(root, "responseBody");
                if (!decodeResult.Success)
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"Decode(...)", "解码失败");
                    return new XResult<SingleSettlementPaymentApplyResponse>(null, ErrorCode.DECODE_FAILED, decodeResult.FirstException);
                }

                var deserializeResult = Deserialize<SingleSettlementPaymentApplyResponse>(decodeResult.Value);
                if (deserializeResult.Success && deserializeResult.Value != null)
                {
                    var result = deserializeResult.Value;
                    result.MemberCode = memberCodeEl.Value;

                    var errorCodeEl = root.Descendants("errorCode").FirstOrDefault();
                    if (errorCodeEl != null)
                    {
                        result.ErrorCode = errorCodeEl.Value;
                    }

                    var errorMsgEl = root.Descendants("errorMsg").FirstOrDefault();
                    if (errorMsgEl != null)
                    {
                        result.ErrorMessage = errorMsgEl.Value;
                    }

                    //将以分为单位的金额转成以元为单位
                    result.Amount *= 0.01m;

                    //更新时间
                    fundoutOrder.UpdateTime = DateTime.Now;
                    //将申请时间、结束时间和更新时间更新到数据库
                    _fundOutOrderRepository.Update(fundoutOrder);
                    var updateResult = _fundOutOrderRepository.SaveChanges();
                    if (!updateResult.Success)
                    {
                        _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(_fundOutOrderRepository)}.SaveChanges()", "更新代付订单申请时间|结束时间|更新时间失败", updateResult.FirstException, fundoutOrder);
                    }

                    return new XResult<SingleSettlementPaymentApplyResponse>(result);
                }

                return new XResult<SingleSettlementPaymentApplyResponse>(null, ErrorCode.DESERIALIZE_FAILED, deserializeResult.FirstException);
            }
            finally
            {
                _lockProvider.UnLock(requestHash);
            }
        }

        public XResult<PagedList<SingleSettlementQueryResponse>> Query(SingleSettlementQueryRequest request)
        {
            if (request == null)
            {
                return new XResult<PagedList<SingleSettlementQueryResponse>>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentNullException($"{nameof(request)}为null"));
            }

            if (!request.IsValid)
            {
                return new XResult<PagedList<SingleSettlementQueryResponse>>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentException(request.ErrorMessage));
            }

            var q = _fundOutOrderRepository.QueryProvider;//.Where(x => x.AppId == request.AppId);

            if (!String.IsNullOrWhiteSpace(request.OrderNo))
            {
                q = q.Where(x => x.OutTradeNo == request.OrderNo);
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
                var ds = q.Select(x => new SingleSettlementQueryResponse()
                {
                    OutTradeNo = x.OutTradeNo,
                    TradeNo = x.TradeNo,
                    Amount = x.Amount,
                    BankCardNo = x.BankCardNo,
                    BankName = x.BankName,
                    CreditName = x.RealName,
                    Fee = x.Fee,
                    FeeAction = x.FeeAction,
                    Status = x.PayStatus,
                    Msg = GetStatusMsg(x.PayStatus),
                    CreateTime = x.CreateTime
                }).OrderByDescending(x => x.CreateTime);
                var result = new PagedList<SingleSettlementQueryResponse>(ds, request.PageIndex, request.PageSize);
                return new XResult<PagedList<SingleSettlementQueryResponse>>(result);
            }
            catch (Exception ex)
            {
                return new XResult<PagedList<SingleSettlementQueryResponse>>(null, ErrorCode.DB_QUERY_FAILED, ex);
            }
        }

        public XResult<PagedList<OrderStatusResult>> QueryStatus(SingleSettlementQueryRequest request)
        {
            if (request == null)
            {
                return new XResult<PagedList<OrderStatusResult>>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentNullException(nameof(request)));
            }

            if (!request.IsValid)
            {
                return new XResult<PagedList<OrderStatusResult>>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentException(request.ErrorMessage));
            }

            var q = _fundOutOrderRepository.QueryProvider;//.Where(x => x.AppId == request.AppId);

            if (!String.IsNullOrWhiteSpace(request.OrderNo))
            {
                q = q.Where(x => x.OutTradeNo == request.OrderNo);
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
                var ds = q.Select(x => new OrderStatusResult()
                {
                    OutTradeNo = x.OutTradeNo,
                    Status = x.PayStatus,
                    Msg = GetStatusMsg(x.PayStatus),
                    CreateTime = x.CreateTime
                }).OrderByDescending(x => x.CreateTime);

                var result = new PagedList<OrderStatusResult>(ds, request.PageIndex, request.PageSize);
                if (result.Exception != null)
                {
                    return new XResult<PagedList<OrderStatusResult>>(null, ErrorCode.DB_QUERY_FAILED, result.Exception);
                }

                return new XResult<PagedList<OrderStatusResult>>(result);
            }
            catch (Exception ex)
            {
                return new XResult<PagedList<OrderStatusResult>>(null, ErrorCode.DB_QUERY_FAILED, ex);
            }
        }

        public XResult<Int32> Pull(Int32 count)
        {
            if (count <= 0 || count > 20)
            {
                return new XResult<Int32>(0, ErrorCode.INVALID_ARGUMENT, new ArgumentOutOfRangeException($"参数count超出范围[1,20]"));
            }

            String service = $"{this.GetType().FullName}.Pull()";

            var key = DateTime.Now.ToString("yyMMddHH").GetHashCode();

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

                List<ApiRequestTimes> orderCreateTimes = null;

                try
                {
                    orderCreateTimes = (from t0 in _fundOutOrderRepository.QueryProvider
                                        where t0.PayStatus != PayStatus.FAILURE.ToString()
                                        && t0.PayStatus != PayStatus.SUCCESS.ToString()
                                        orderby t0.CreateTime
                                        select new ApiRequestTimes(t0.OutTradeNo, t0.ApplyTime, t0.EndTime, t0.CreateTime)).Take(count).ToList();
                }
                catch (Exception ex)
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, "orderCreateTimes", "查询待处理的代付订单记录失败", ex);
                    return new XResult<Int32>(0);
                }

                if (orderCreateTimes == null || orderCreateTimes.Count == 0)
                {
                    return new XResult<Int32>(0);
                }

                var first = orderCreateTimes.First();
                var end = orderCreateTimes.Last();

                DateTime startTime = first.ApplyTime != null ? first.ApplyTime.Value : first.CreateTime;
                DateTime endTime = end.EndTime != null ? end.EndTime.Value : end.CreateTime.AddMinutes(2);

                if ((endTime - startTime).TotalSeconds < 5)
                {
                    endTime = startTime.AddSeconds(5);
                    startTime = startTime.AddSeconds(-5);
                }

                var queryResult = Query99bill(new Bill99SingleSettlementQueryRequest()
                {
                    PageIndex = 1,
                    PageSize = count,
                    StartTime = startTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    EndTime = endTime.ToString("yyyy-MM-dd HH:mm:ss")
                });

                StringBuilder sb = new StringBuilder();

                if (!queryResult.Success && queryResult.Exceptions.Count > 0)
                {
                    //如果快钱返回Q0018:没有查询到结果，且订单创建时间超过了2小时
                    //说明该订单没有在快钱创建成功，则将该订单状态设置为失败
                    if (queryResult.FirstException.Message.IndexOf("Q0018:") >= 0)
                    {
                        var expiredOrders = orderCreateTimes.Where(x => DateTime.Now > x.CreateTime.AddHours(2));
                        if (expiredOrders != null && expiredOrders.Count() > 0)
                        {
                            foreach (var order in expiredOrders)
                            {
                                sb.Append($"UPDATE fundout_order SET pay_status='{PayStatus.FAILURE.ToString()}', update_time='{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}' WHERE out_trade_no='{order.OrderNo}';");
                            }
                        }
                    }
                    else
                    {
                        return new XResult<Int32>(0, ErrorCode.DB_QUERY_FAILED, queryResult.FirstException);
                    }
                }

                var resultList = queryResult.Value;
                if (resultList != null)
                {
                    foreach (var item in resultList)
                    {
                        var status = GetPayStatus(item.Status);
                        if (status != PayStatus.SUCCESS
                            && status != PayStatus.FAILURE)
                        {
                            continue;
                        }

                        if (!Regex.IsMatch(item.OrderNo, @"^[a-z0-9\-_]+$", RegexOptions.IgnoreCase)) { continue; }
                        sb.Append($"UPDATE fundout_order SET pay_status='{GetPayStatus(item.Status).ToString()}', fee='{item.Fee.ToString()}', update_time='{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}' WHERE out_trade_no='{item.OrderNo}';");
                    }
                }

                if (sb.Length > 0)
                {
                    String sql = sb.ToString();
                    String traceMethod = $"{nameof(_fundOutOrderRepository)}.ExecuteSql(...)";
                    _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.OK.ToString(), service, traceMethod, LogPhase.BEGIN, $"开始执行SQL语句", sql);
                    var execResult = _fundOutOrderRepository.ExecuteSql(FormattableStringFactory.Create(sql));
                    _logger.Trace(TraceType.BLL.ToString(), (execResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), service, traceMethod, LogPhase.END, "完成执行SQL语句", $"受影响{execResult.Value}行");
                    return execResult;
                }

                return new XResult<Int32>(0);
            }
            finally
            {
                _lockProvider.UnLock(key);
            }
        }

        private XResult<IEnumerable<Bill99SingleSettlementQueryResponse>> Query99bill(Bill99SingleSettlementQueryRequest request)
        {
            String service = $"{this.GetType().FullName}.Query99bill(...)";

            if (!request.IsValid)
            {
                _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(request)}.IsValid", "查询代付结果的参数验证失败", null, request);
                return new XResult<IEnumerable<Bill99SingleSettlementQueryResponse>>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentException(request.ErrorMessage));
            }

            var requestBuilder = new RequestBuilder(_serializer, _logger)
            {
                EncryptKey = CryptoHelper.GenerateRandomKey(),
                RequestType = "pay2BankSearchRequest",
                RequestHead = "pay2bankSearchHead",
                RequestBodyType = "searchRequestBody",
                RequestBody = request
            };

            String postXml = requestBuilder.Build();

            String traceMethod = $"{nameof(_client)}.PostXml(...)";

            _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.OK.ToString(), service, traceMethod, LogPhase.BEGIN, $"开始调用{traceMethod}", postXml);

            var postResult = _client.PostXml(ApiConfig.Bill99FOSingleQueryRequestUrl, postXml);

            _logger.Trace(TraceType.BLL.ToString(), (postResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), service, traceMethod, LogPhase.ACTION, $"完成调用{traceMethod}");

            if (!postResult.Success)
            {
                return new XResult<IEnumerable<Bill99SingleSettlementQueryResponse>>(null, ErrorCode.DEPENDENT_API_CALL_FAILED, postResult.FirstException);
            }

            String resp = null;
            try
            {
                resp = postResult.Value.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.OK.ToString(), service, traceMethod, LogPhase.END, $"调用{traceMethod}返回结果", resp);
            }
            catch (Exception ex)
            {
                _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, "postResult.Value.Content.ReadAsStringAsync()", "读取响应内容出现异常", ex);
                return new XResult<IEnumerable<Bill99SingleSettlementQueryResponse>>(null, ErrorCode.RESPONSE_READ_FAILED, ex);
            }

            if (String.IsNullOrWhiteSpace(resp))
            {
                return new XResult<IEnumerable<Bill99SingleSettlementQueryResponse>>(null, ErrorCode.REMOTE_RETURN_NOTHING);
            }

            var parseResult = ParseXml(resp);
            if (!parseResult.Success)
            {
                _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, "ParseXml(...)", "解析XML失败", parseResult.FirstException, resp);
                return new XResult<IEnumerable<Bill99SingleSettlementQueryResponse>>(null, ErrorCode.DESERIALIZE_FAILED, parseResult.FirstException);
            }

            var memberCodeEl = parseResult.Value.Root.Descendants("memberCode").FirstOrDefault();
            if (memberCodeEl == null)
            {
                _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, "memberCodeEl", $"返回数据中未包含<memberCode></memberCode>元素", null, parseResult.Value.Root);
                return new XResult<IEnumerable<Bill99SingleSettlementQueryResponse>>(null, ErrorCode.XML_ELEMENT_NOT_EXIST, new RemoteException("<memberCode> not found"));
            }

            var decodeResult = Decode(parseResult.Value.Root, "searchResponseBody");
            if (!decodeResult.Success)
            {
                _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"Decode(...)", $"解码失败", parseResult.FirstException, parseResult.Value.Root);
                return new XResult<IEnumerable<Bill99SingleSettlementQueryResponse>>(null, ErrorCode.DECODE_FAILED, decodeResult.FirstException);
            }

            parseResult = ParseXml(decodeResult.Value);
            if (!parseResult.Success)
            {
                return new XResult<IEnumerable<Bill99SingleSettlementQueryResponse>>(null, ErrorCode.XML_PARSE_FAILED, parseResult.FirstException);
            }

            var resultListEls = parseResult.Value.Root.Elements("resultList");
            if (resultListEls == null || resultListEls.Count() == 0)
            {
                return new XResult<IEnumerable<Bill99SingleSettlementQueryResponse>>(null, ErrorCode.XML_ELEMENT_NOT_EXIST, new RemoteException("<resultList> not found"));
            }

            List<Bill99SingleSettlementQueryResponse> results = new List<Bill99SingleSettlementQueryResponse>(resultListEls.Count());
            foreach (var resultListEl in resultListEls)
            {
                var deserializeResult = Deserialize<Bill99SingleSettlementQueryResponse>(resultListEl.ToString(SaveOptions.DisableFormatting));
                if (deserializeResult.Success && deserializeResult.Value != null)
                {
                    var result = deserializeResult.Value;
                    result.MemberCode = memberCodeEl.Value;

                    var errorCodeEl = resultListEl.Element("errorCode");
                    if (errorCodeEl != null)
                    {
                        result.ErrorCode = errorCodeEl.Value;
                    }

                    var errorMsgEl = resultListEl.Element("errorMsg");
                    if (errorMsgEl != null)
                    {
                        result.ErrorMessage = errorMsgEl.Value;
                    }

                    //将以分为单位的金额转成以元为单位
                    deserializeResult.Value.Amount *= 0.01m;
                    deserializeResult.Value.Fee *= 0.01m;
                }

                results.Add(deserializeResult.Value);
            }

            return new XResult<IEnumerable<Bill99SingleSettlementQueryResponse>>(results);
        }

        private XResult<String> Decode(XElement root, String responseBodyType)
        {
            String service = $"{this.GetType().FullName}.Decode(...)";

            if (root == null)
            {
                return new XResult<String>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentNullException(nameof(root)));
            }

            var responseBodyEl = root.Descendants(responseBodyType).FirstOrDefault();
            if (responseBodyEl == null)
            {
                return new XResult<String>(null, ErrorCode.XML_ELEMENT_NOT_EXIST, new RemoteException($"<{responseBodyType}> not found"));
            }

            var errorCodeEl = responseBodyEl.Element("errorCode");
            if (errorCodeEl != null && errorCodeEl.Value != "0000")
            {
                var errorMsgEl = responseBodyEl.Element("errorMsg");
                var errorMsg = $"{errorCodeEl.Value}:{errorMsgEl.Value}";
                return new XResult<String>(null, ErrorCode.DEPENDENT_API_CALL_FAILED, new RemoteException(errorMsg));
            }

            var digitalEnvEl = responseBodyEl.Descendants("digitalEnvelope").FirstOrDefault();
            if (digitalEnvEl == null || String.IsNullOrWhiteSpace(digitalEnvEl.Value))
            {
                return new XResult<String>(null, ErrorCode.XML_ELEMENT_NOT_EXIST, new RemoteException("<digitalEnvelope> not found"));
            }

            Byte[] digitalEnvData = null;
            try
            {
                digitalEnvData = Convert.FromBase64String(digitalEnvEl.Value);
            }
            catch (Exception ex)
            {
                _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, "Convert.FromBase64String(...)", "数字信封的值不是有效的Base64字符串");
                return new XResult<String>(null, ErrorCode.DECODE_FAILED, ex);
            }

            Byte[] key = null;
            using (var ms = new MemoryStream(digitalEnvData))
            {
                var decryptKeyResult = CryptoHelper.RSADecrypt(ms, KeyConfig.Bill99FOHehuaPrivateKey, PrivateKeyFormat.PKCS8);
                if (!decryptKeyResult.Success)
                {
                    return new XResult<String>(null, ErrorCode.DECRYPT_FAILED, decryptKeyResult.FirstException);
                }

                key = decryptKeyResult.Value;
            }

            var encryptedDataEl = responseBodyEl.Descendants("encryptedData").FirstOrDefault();
            if (encryptedDataEl == null)
            {
                return new XResult<String>(null, ErrorCode.XML_ELEMENT_NOT_EXIST, new RemoteException("<encryptedData> not found"));
            }

            Byte[] encryptedData = null;
            try
            {
                encryptedData = Convert.FromBase64String(encryptedDataEl.Value);
            }
            catch (Exception ex)
            {
                _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, "Convert.FromBase64String(...)", "encryptedData不是有效的Base64字符串");
                return new XResult<String>(null, ErrorCode.DECODE_FAILED, ex);
            }

            var decryptedResult = CryptoHelper.AESDecrypt(encryptedData, key);
            if (!decryptedResult.Success)
            {
                return new XResult<String>(null, ErrorCode.DECRYPT_FAILED, decryptedResult.FirstException);
            }

            var signedDataEl = responseBodyEl.Descendants("signedData").FirstOrDefault();
            if (signedDataEl == null)
            {
                return new XResult<String>(null, ErrorCode.XML_ELEMENT_NOT_EXIST, new RemoteException("<signedData> not found"));
            }

            Byte[] sign = null;
            try
            {
                sign = Convert.FromBase64String(signedDataEl.Value);
            }
            catch (Exception ex)
            {
                _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, "Convert.FromBase64String(...)", "signedData不是有效的Base64字符串", ex);
                return new XResult<String>(null, ErrorCode.DECODE_FAILED, new RemoteException("signedData不是有效的Base64字符串"));
            }

            Byte[] signContent = decryptedResult.Value;

            var verifyResult = CryptoHelper.VerifySign(sign, signContent, KeyConfig.Bill99FOPublicKey, HashAlgorithmName.SHA1);
            if (!verifyResult.Value)
            {
                _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, "verifyResult", "验签失败", verifyResult.FirstException);
                return new XResult<String>(null, ErrorCode.SIGN_VERIFY_FAILED, new SignException("sign verify failed"));
            }

            try
            {
                String resultXml = Encoding.UTF8.GetString(decryptedResult.Value);
                _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.OK.ToString(), service, "resultXml", LogPhase.ACTION, "代付解析结果", resultXml);

                return new XResult<String>(resultXml);
            }
            catch (Exception ex)
            {
                return new XResult<String>(null, ErrorCode.DECODE_FAILED, ex);
            }
        }

        private XResult<T> Deserialize<T>(String xml) where T : FOCommonResponse
        {
            try
            {
                var result = _serializer.Deserialize<T>(xml);
                return new XResult<T>(result);
            }
            catch (Exception ex)
            {
                return new XResult<T>(null, ErrorCode.DESERIALIZE_FAILED, ex);
            }
        }

        private XResult<XDocument> ParseXml(String response)
        {
            XDocument doc = null;
            try
            {
                doc = XDocument.Parse(response);
                return new XResult<XDocument>(doc);
            }
            catch (Exception ex)
            {
                return new XResult<XDocument>(null, ErrorCode.XML_PARSE_FAILED, ex);
            }
        }

        private PayStatus GetPayStatus(String status)
        {
            switch (status)
            {
                case "101":
                    return PayStatus.PROCESSING;
                case "111":
                    return PayStatus.SUCCESS;
                case "112":
                    return PayStatus.FAILURE;
            }

            return PayStatus.APPLY;
        }

        private String GetStatusMsg(String status)
        {
            switch (status)
            {
                case "SUCCESS":
                    return PayStatus.SUCCESS.GetDescription();
                case "FAILURE":
                    return PayStatus.FAILURE.GetDescription();
                case "PROCESSING":
                    return PayStatus.PROCESSING.GetDescription();
                case "APPLY":
                    return PayStatus.APPLY.GetDescription();
            }

            return null;
        }

        private static HttpClient CreateHttpClient()
        {
            var factory = XDI.Resolve<IHttpClientFactory>();
            return factory.CreateClient();
        }
    }

    internal struct ApiRequestTimes
    {
        public ApiRequestTimes(String orderNo, DateTime? applyTime, DateTime? endTime, DateTime createTime)
        {
            this.OrderNo = orderNo;
            this.ApplyTime = applyTime;
            this.EndTime = endTime;
            this.CreateTime = createTime;
        }

        public String OrderNo { get; }
        public DateTime? ApplyTime { get; }
        public DateTime? EndTime { get; }
        public DateTime CreateTime { get; }
    }

    internal class RequestBuilder
    {
        private readonly XSerializer _serializer;
        private readonly ILogger _logger;

        public RequestBuilder(XSerializer serializer, ILogger logger)
        {
            if (serializer == null)
            {
                throw new ArgumentNullException(nameof(serializer));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            _serializer = serializer;
            _logger = logger;
        }

        public String RequestType { get; set; }
        public String RequestHead { get; set; }
        public String RequestBodyType { get; set; }
        public Object RequestBody { get; set; }
        public Byte[] EncryptKey { get; set; }

        public String Build(Action<XDocument> configAction = null)
        {
            String service = $"{this.GetType().FullName}.Build(...)";

            String xmlString = _serializer.Serialize(this.RequestBody, configAction);

            _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.OK.ToString(), service, "xmlString", LogPhase.ACTION, "代付请求参数", xmlString);

            Byte[] xmlData = Encoding.UTF8.GetBytes(xmlString);

            //签名数据
            var signedResult = SignUtil.MakeSign(xmlData, KeyConfig.Bill99FOHehuaPrivateKey, PrivateKeyFormat.PKCS8, "RSA");
            if (!signedResult.Success)
            {
                _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, "signedResult", "生成签名数据失败", signedResult.FirstException, xmlString);
                return null;
            }

            //密文
            var encryptedResult = CryptoHelper.AESEncrypt(xmlData, this.EncryptKey);
            if (!encryptedResult.Success)
            {
                _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, "encryptedResult", "生成密文失败", encryptedResult.FirstException, xmlString);
                return null;
            }

            //数字信封
            var digResult = CryptoHelper.RSAEncrypt(this.EncryptKey, KeyConfig.Bill99FOPublicKey);
            if (!digResult.Success)
            {
                _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, "digResult", "生成数字信封失败", digResult.FirstException);
                return null;
            }

            String signedData = Convert.ToBase64String(signedResult.Value);
            String encryptedData = Convert.ToBase64String(encryptedResult.Value);
            String digitalEnvelope = Convert.ToBase64String(digResult.Value);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?>");
            sb.Append($"<{this.RequestType}>");
            sb.Append($"<{this.RequestHead}>");
            sb.Append("<version>1.0</version>");
            sb.Append($"<memberCode>{GlobalConfig.X99bill_FundOut_Hehua_MemberCode}</memberCode>");
            //sb.Append($"<memberCode>10012138842</memberCode>");
            sb.Append($"</{this.RequestHead}>");
            sb.Append($"<{this.RequestBodyType}>");
            sb.Append("<sealDataType>");
            sb.Append($"<originalData></originalData>");
            sb.Append($"<signedData>{signedData}</signedData>");
            sb.Append($"<encryptedData>{encryptedData}</encryptedData>");
            sb.Append($"<digitalEnvelope>{digitalEnvelope}</digitalEnvelope>");
            sb.Append("</sealDataType>");
            sb.Append($"</{this.RequestBodyType}>");
            sb.Append($"</{this.RequestType}>");

            return sb.ToString();
        }
    }

}
