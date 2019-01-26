using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPI.Common;
using CPI.Common.Domain.SettleDomain.Bill99;
using CPI.Common.Exceptions;
using CPI.Common.Models;
using CPI.Config;
using CPI.IData.BaseRepositories;
using CPI.IService.SettleServices;
using CPI.Providers;
using CPI.Utils;
using Lotus.Core;
using Lotus.Core.Collections;
using Lotus.Logging;

namespace CPI.Services.SettleServices
{
    public class WithdrawService : IWithdrawService
    {
        private static readonly LockProvider _lockProvider = new LockProvider();
        private static readonly ILogger _logger = LogManager.GetLogger();

        private readonly IAllotAmountWithdrawOrderRepository _allotAmountWithdrawOrderRepository = null;
        private readonly IWithdrawBankCardBindInfoRepository _withdrawBankCardBindInfoRepository = null;

        public XResult<WithdrawQueryResponse> QueryDetails(WithdrawQueryRequest request)
        {
            if (request == null)
            {
                return new XResult<WithdrawQueryResponse>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentNullException(nameof(request)));
            }

            if (!request.IsValid)
            {
                return new XResult<WithdrawQueryResponse>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentException(request.ErrorMessage));
            }

            String service = $"{this.GetType().FullName}.QueryDetails(...)";

            var existedInfo = _allotAmountWithdrawOrderRepository.QueryProvider.FirstOrDefault(x => x.PayeeId == request.PayeeId && x.OutTradeNo == request.OutTradeNo);
            if (existedInfo == null)
            {
                var queryResult = Bill99Util.Execute<RawWithdrawQueryRequest, RawWithdrawQueryResponse>("/withdraw/query", new RawWithdrawQueryRequest()
                {
                    uId = request.PayeeId,
                    outTradeNo = request.OutTradeNo
                });

                if (queryResult.Success && queryResult.Value != null && queryResult.Value.ResponseCode == "0000")
                {
                    if (queryResult.Value.BankCardNo.HasValue())
                    {
                        _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, nameof(existedInfo), "数据库中不存在提现记录但快钱返回存在", null, new Object[] { request, queryResult.Value });
                    }
                }

                return new XResult<WithdrawQueryResponse>(null, ErrorCode.INFO_NOT_EXIST, new ArgumentException("提现记录不存在"));
            }

            if (existedInfo.Status != WithdrawOrderStatus.SUCCESS.ToString() && existedInfo.Status != WithdrawOrderStatus.FAILURE.ToString())
            {
                var queryResult = Bill99Util.Execute<RawWithdrawQueryRequest, RawWithdrawQueryResponse>("/withdraw/query", new RawWithdrawQueryRequest()
                {
                    uId = request.PayeeId,
                    outTradeNo = request.OutTradeNo
                });

                if (queryResult.Success && queryResult.Value != null && queryResult.Value.ResponseCode == "0000")
                {
                    Boolean statusHasChanged = false;

                    if (queryResult.Value.Status == "1")
                    {
                        existedInfo.Status = WithdrawOrderStatus.SUCCESS.ToString();
                        _allotAmountWithdrawOrderRepository.Update(existedInfo);
                        statusHasChanged = true;
                    }
                    else if (queryResult.Value.Status == "2")
                    {
                        existedInfo.Status = WithdrawOrderStatus.FAILURE.ToString();
                        _allotAmountWithdrawOrderRepository.Update(existedInfo);
                        statusHasChanged = true;
                    }

                    if (statusHasChanged)
                    {
                        var updateStatusResult = _allotAmountWithdrawOrderRepository.SaveChanges();
                        if (!updateStatusResult.Success)
                        {
                            _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(_allotAmountWithdrawOrderRepository)}.SaveChanges()", "更新提现状态失败", updateStatusResult.FirstException, existedInfo);
                        }
                    }
                }
            }

            var bindcardinfo = _withdrawBankCardBindInfoRepository.QueryProvider.FirstOrDefault(x => x.PayeeId == request.PayeeId);
            if (bindcardinfo == null)
            {
                return new XResult<WithdrawQueryResponse>(null, SettleErrorCode.NO_BANKCARD_BOUND);
            }

            return new XResult<WithdrawQueryResponse>(new WithdrawQueryResponse()
            {
                Amount = existedInfo.Amount,
                BankCardNo = bindcardinfo.BankCardNo,
                CustomerFee = existedInfo.CustomerFee,
                MemberBankAcctId = bindcardinfo.MemberBankAccountId,
                MerchantFee = existedInfo.MerchantFee,
                OutTradeNo = existedInfo.OutTradeNo,
                Status = existedInfo.Status,
                Msg = GetWithdrawOrderStatusMsg(existedInfo.Status)
            });
        }

        public XResult<WithdrawQueryFeeResponse> QueryFee(WithdrawQueryFeeRequest request)
        {
            if (request == null)
            {
                return new XResult<WithdrawQueryFeeResponse>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentNullException(nameof(request)));
            }

            if (!request.IsValid)
            {
                return new XResult<WithdrawQueryFeeResponse>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentException(request.ErrorMessage));
            }

            var queryResult = Bill99Util.Execute<RawWithdrawQueryFeeRequest, RawWithdrawQueryFeeResponse>("/withdraw/queryFee", new RawWithdrawQueryFeeRequest()
            {
                uId = request.PayeeId,
                amount = request.Amount
            });

            if (!queryResult.Success)
            {
                return new XResult<WithdrawQueryFeeResponse>(null, ErrorCode.FAILURE, queryResult.FirstException);
            }

            if (queryResult.Value == null)
            {
                return new XResult<WithdrawQueryFeeResponse>(null, ErrorCode.REMOTE_RETURN_NOTHING);
            }

            if (queryResult.Value.ResponseCode != "0000")
            {
                return new XResult<WithdrawQueryFeeResponse>(null, ErrorCode.FAILURE, new RemoteException(queryResult.Value.ResponseMessage));
            }

            return new XResult<WithdrawQueryFeeResponse>(new WithdrawQueryFeeResponse()
            {
                Fee = queryResult.Value.Fee,
                Status = CommonStatus.SUCCESS.ToString(),
                Msg = CommonStatus.SUCCESS.GetDescription()
            });
        }

        public XResult<PagedList<WithdrawStatusQueryResult>> QueryStatus(WithdrawStatusQueryRequest request)
        {
            if (request == null)
            {
                return new XResult<PagedList<WithdrawStatusQueryResult>>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentNullException(nameof(request)));
            }

            String service = $"{this.GetType().FullName}.QueryStatus(...)";

            if (!request.IsValid)
            {
                _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(request)}.IsValid", LogPhase.ACTION, $"请求参数验证失败：{request.ErrorMessage}", request);
                return new XResult<PagedList<WithdrawStatusQueryResult>>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentException(request.ErrorMessage));
            }

            var q = _allotAmountWithdrawOrderRepository.QueryProvider;//.Where(x => x.AppId == request.AppId);

            if (!String.IsNullOrWhiteSpace(request.OutTradeNo))
            {
                q = q.Where(x => x.OutTradeNo == request.OutTradeNo);
            }

            if (request.From != null)
            {
                q = q.Where(x => x.ApplyTime >= request.From.Value);
            }

            if (request.To != null)
            {
                q = q.Where(x => x.ApplyTime <= request.To.Value);
            }

            try
            {
                var ds = q.Select(x => new WithdrawStatusQueryResult()
                {
                    OutTradeNo = x.OutTradeNo,
                    Status = x.Status,
                    Msg = GetWithdrawOrderStatusMsg(x.Status),
                    Amount = x.Amount,
                    ApplyTime = x.ApplyTime
                }).OrderByDescending(x => x.ApplyTime);

                var result = new PagedList<WithdrawStatusQueryResult>(ds, request.PageIndex, request.PageSize);
                if (result.Exception != null)
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, "ds", "查询提现状态出现异常", result.Exception);
                    return new XResult<PagedList<WithdrawStatusQueryResult>>(null, ErrorCode.DB_QUERY_FAILED, result.Exception);
                }

                return new XResult<PagedList<WithdrawStatusQueryResult>>(result);
            }
            catch (Exception ex)
            {
                _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, ":", "查询提现状态出现异常", ex);
                return new XResult<PagedList<WithdrawStatusQueryResult>>(null, ErrorCode.DB_QUERY_FAILED, ex);
            }

        }

        public XResult<WithdrawResponse> Withdraw(WithdrawRequest request)
        {
            if (request == null)
            {
                return new XResult<WithdrawResponse>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentNullException(nameof(request)));
            }

            String service = $"{this.GetType().FullName}.Withdraw(...)";

            if (!request.IsValid)
            {
                _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(request)}.IsValid", LogPhase.ACTION, $"请求参数验证失败：{request.ErrorMessage}", request);
                return new XResult<WithdrawResponse>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentException(request.ErrorMessage));
            }

            if (request.Amount < GlobalConfig.X99bill_YZT_WithdrawMinAmount)
            {
                return new XResult<WithdrawResponse>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentException($"提现金额至少为{GlobalConfig.X99bill_YZT_WithdrawMinAmount.ToString()}元"));
            }

            var requestHash = $"withdraw:{request.PayeeId}".GetHashCode();

            if (_lockProvider.Exists(requestHash))
            {
                return new XResult<WithdrawResponse>(null, ErrorCode.SUBMIT_REPEAT);
            }

            try
            {
                if (!_lockProvider.Lock(requestHash))
                {
                    return new XResult<WithdrawResponse>(null, ErrorCode.SUBMIT_REPEAT);
                }

                var existedOrder = _allotAmountWithdrawOrderRepository.QueryProvider.FirstOrDefault(x => x.OutTradeNo == request.OutTradeNo);
                if (existedOrder == null)
                {
                    var newId = IDGenerator.GenerateID();
                    existedOrder = new AllotAmountWithdrawOrder()
                    {
                        Id = newId,
                        AppId = request.AppId,
                        TradeNo = newId.ToString(),
                        PayeeId = request.PayeeId,
                        OutTradeNo = request.OutTradeNo,
                        Amount = request.Amount,
                        SettlePeriod = request.SettlePeriod,
                        CustomerFee = request.CustomerFee,
                        MerchantFee = request.MerchantFee,
                        Status = WithdrawOrderStatus.APPLY.ToString(),
                        ApplyTime = DateTime.Now
                    };

                    _allotAmountWithdrawOrderRepository.Add(existedOrder);

                    var saveResult = _allotAmountWithdrawOrderRepository.SaveChanges();
                    if (!saveResult.Success)
                    {
                        _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(_allotAmountWithdrawOrderRepository)}.SaveChanges()", "保存分账提现记录失败", saveResult.FirstException, existedOrder);
                        return new XResult<WithdrawResponse>(null, ErrorCode.DB_UPDATE_FAILED, saveResult.FirstException);
                    }
                }

                if (existedOrder.Status == WithdrawOrderStatus.SUCCESS.ToString()
                    || existedOrder.Status == WithdrawOrderStatus.FAILURE.ToString())
                {
                    return new XResult<WithdrawResponse>(null, ErrorCode.OUT_TRADE_NO_EXISTED);
                }

                String traceMethod = $"Bill99Util.Execute(/account/withdraw)";

                _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.OK.ToString(), service, traceMethod, LogPhase.BEGIN);

                var execResult = Bill99Util.Execute<RawWithdrawRequest, RawWithdrawResponse>("/account/withdraw", new RawWithdrawRequest()
                {
                    uId = request.PayeeId,
                    outTradeNo = request.OutTradeNo,
                    amount = request.Amount,
                    customerFee = request.CustomerFee,
                    merchantFee = request.MerchantFee
                });

                _logger.Trace(TraceType.BLL.ToString(), (execResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), service, traceMethod, LogPhase.END);

                if (!execResult.Success)
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, traceMethod, "申请分账提现失败", execResult.FirstException, request);
                    return new XResult<WithdrawResponse>(null, ErrorCode.DEPENDENT_API_CALL_FAILED, execResult.FirstException);
                }

                var resp = execResult.Value;

                if (resp == null)
                {
                    _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(execResult)}.Value", LogPhase.ACTION, "快钱未返回任何数据");
                    return new XResult<WithdrawResponse>(null, ErrorCode.REMOTE_RETURN_NOTHING);
                }

                if (resp.ResponseCode != "0000")
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(resp)}.ResponseCode", $"申请分账提现失败：{resp.ResponseCode}:{resp.ResponseMessage}");
                    return new XResult<WithdrawResponse>(null, ErrorCode.DEPENDENT_API_CALL_FAILED, new RemoteException($"{resp.ResponseCode}:{resp.ResponseMessage}"));
                }

                Boolean statusHasChanged = false;

                switch (resp.status)
                {
                    case "1":
                        existedOrder.Status = WithdrawOrderStatus.SUCCESS.ToString();
                        existedOrder.CompleteTime = DateTime.Now;
                        statusHasChanged = true;
                        break;
                    case "2":
                        existedOrder.Status = WithdrawOrderStatus.FAILURE.ToString();
                        existedOrder.CompleteTime = DateTime.Now;
                        statusHasChanged = true;
                        break;
                    case "3":
                        existedOrder.Status = WithdrawOrderStatus.PROCESSING.ToString();
                        statusHasChanged = true;
                        break;
                }

                if (statusHasChanged)
                {
                    _allotAmountWithdrawOrderRepository.Update(existedOrder);
                    var updateResult = _allotAmountWithdrawOrderRepository.SaveChanges();
                    if (!updateResult.Success)
                    {
                        _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(_allotAmountWithdrawOrderRepository)}.SaveChanges()", "更新申请提交结果失败", updateResult.FirstException, existedOrder);
                    }
                }

                return new XResult<WithdrawResponse>(new WithdrawResponse()
                {
                    Status = existedOrder.Status,
                    Msg = GetWithdrawOrderStatusMsg(existedOrder.Status)
                });
            }
            finally
            {
                _lockProvider.UnLock(requestHash);
            }
        }

        private String GetWithdrawOrderStatusMsg(String status)
        {
            switch (status)
            {
                case "SUCCESS":
                    return WithdrawOrderStatus.SUCCESS.GetDescription();
                case "FAILURE":
                    return WithdrawOrderStatus.FAILURE.GetDescription();
                case "PROCESSING":
                    return WithdrawOrderStatus.PROCESSING.GetDescription();
                case "APPLY":
                    return WithdrawOrderStatus.APPLY.GetDescription();
            }

            return null;
        }

    }
}
