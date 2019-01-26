using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPI.Common;
using CPI.Common.Domain.SettleDomain.Bill99;
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
    public class AllotAmountService : IAllotAmountService
    {
        private static readonly LockProvider _lockProvider = new LockProvider();
        private static readonly ILogger _logger = LogManager.GetLogger();

        private readonly IAllotAmountOrderRepository _allotAmountOrderRepository = null;
        private readonly IAllotAmountWithdrawOrderRepository _allotAmountWithdrawOrderRepository = null;

        public XResult<AllotAmountPayResponse> Pay(AllotAmountPayRequest request)
        {
            if (request == null)
            {
                return new XResult<AllotAmountPayResponse>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentNullException(nameof(request)));
            }

            String service = $"{this.GetType().FullName}.Pay(...)";

            if (!request.IsValid)
            {
                return new XResult<AllotAmountPayResponse>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentException(request.ErrorMessage));
            }

            var requestHash = $"allot.pay:{request.WithdrawOutTradeNo}".GetHashCode();

            if (_lockProvider.Exists(requestHash))
            {
                return new XResult<AllotAmountPayResponse>(null, ErrorCode.SUBMIT_REPEAT);
            }

            try
            {
                if (!_lockProvider.Lock(requestHash))
                {
                    return new XResult<AllotAmountPayResponse>(null, ErrorCode.SUBMIT_REPEAT);
                }

                //判断提现单是否存在
                var withdrawOrderIsExisted = _allotAmountWithdrawOrderRepository.Exists(x => x.OutTradeNo == request.WithdrawOutTradeNo);
                if (!withdrawOrderIsExisted)
                {
                    return new XResult<AllotAmountPayResponse>(null, ErrorCode.INFO_NOT_EXIST, new ArgumentException("提现单不存在"));
                }

                //得到与提现单对应的正在处理中的分账单
                var existedOrder = _allotAmountOrderRepository.QueryProvider.FirstOrDefault(x => x.WithdrawOutTradeNo == request.WithdrawOutTradeNo
                && x.Status != AllotAmountOrderStatus.SUCCESS.ToString()
                && x.Status != AllotAmountOrderStatus.FAILURE.ToString()
                && x.AllotType == AllotAmountType.Pay.ToString());
                if (existedOrder != null)
                {
                    return new XResult<AllotAmountPayResponse>(null, ErrorCode.INFO_EXISTED, new ArgumentException("消费分账订单已存在"));
                }

                var newId = IDGenerator.GenerateID();
                existedOrder = new AllotAmountOrder()
                {
                    Id = newId,
                    AppId = request.AppId,
                    TradeNo = newId.ToString(),
                    PayeeId = request.PayeeId,
                    WithdrawOutTradeNo = request.WithdrawOutTradeNo,
                    OutTradeNo = newId.ToString(),
                    TotalAmount = request.TotalAmount,
                    AllotType = AllotAmountType.Pay.ToString(),
                    SettlePeriod = request.SettlePeriod,
                    Status = AllotAmountOrderStatus.APPLY.ToString(),
                    ApplyTime = DateTime.Now
                };

                _allotAmountOrderRepository.Add(existedOrder);
                var saveResult = _allotAmountOrderRepository.SaveChanges();
                if (!saveResult.Success)
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(_allotAmountOrderRepository)}.SaveChanges()", "保存消费分账单失败", saveResult.FirstException, existedOrder);
                    return new XResult<AllotAmountPayResponse>(null, ErrorCode.DB_UPDATE_FAILED, new DbUpdateException("保存消费分账单失败"));
                }

                String traceMethod = $"Bill99Util.Execute(/settle/pay)";

                _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.OK.ToString(), service, traceMethod, LogPhase.BEGIN);

                var settleData = new RawSettleData[1];
                settleData[0] = new RawSettleData()
                {
                    amount = request.TotalAmount,
                    merchantUid = request.PayeeId,
                    outSubOrderNo = existedOrder.OutTradeNo,
                    settlePeriod = request.SettlePeriod
                };

                var execResult = Bill99Util.Execute<RawAllotAmountPayRequest, RawAllotAmountPayResponse>("/settle/pay", new RawAllotAmountPayRequest()
                {
                    outOrderNo = existedOrder.OutTradeNo,
                    settleData = settleData,
                    totalAmount = request.TotalAmount
                });

                _logger.Trace(TraceType.BLL.ToString(), (execResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), service, traceMethod, LogPhase.END);

                if (!execResult.Success)
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, traceMethod, "申请消费分账失败", execResult.FirstException, request);
                    return new XResult<AllotAmountPayResponse>(null, ErrorCode.DEPENDENT_API_CALL_FAILED, execResult.FirstException);
                }

                if (execResult.Value.ResponseCode != "0000")
                {
                    _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, traceMethod, LogPhase.ACTION, "申请消费分账失败", new Object[] { request, execResult.Value });
                    return new XResult<AllotAmountPayResponse>(null, ErrorCode.DEPENDENT_API_CALL_FAILED, new RemoteException($"{execResult.Value.ResponseCode}:{execResult.Value.ResponseMessage}"));
                }

                existedOrder.Status = AllotAmountOrderStatus.PROCESSING.ToString();
                _allotAmountOrderRepository.Update(existedOrder);

                var updateResult = _allotAmountOrderRepository.SaveChanges();
                if (!updateResult.Success)
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(_allotAmountOrderRepository)}.SaveChanges()", "更新分账结果失败", updateResult.FirstException, existedOrder);
                }

                return new XResult<AllotAmountPayResponse>(new AllotAmountPayResponse()
                {
                    Status = existedOrder.Status,
                    Msg = GetAllotAmountOrderStatusMsg(existedOrder.Status)
                });
            }
            finally
            {
                _lockProvider.UnLock(requestHash);
            }
        }

        public XResult<AllotAmountResultQueryResponse> Query(AllotAmountResultQueryRequest request)
        {
            return Bill99Util.Execute<AllotAmountResultQueryRequest, AllotAmountResultQueryResponse>("/settle/detail", request);
        }

        public XResult<AllotAmountRefundResponse> Refund(AllotAmountRefundRequest request)
        {
            if (request == null)
            {
                return new XResult<AllotAmountRefundResponse>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentNullException(nameof(request)));
            }

            String service = $"{this.GetType().FullName}.Refund(...)";

            if (!request.IsValid)
            {
                return new XResult<AllotAmountRefundResponse>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentException(request.ErrorMessage));
            }

            var requestHash = $"allot.refund:{request.WithdrawOutTradeNo}".GetHashCode();

            if (_lockProvider.Exists(requestHash))
            {
                return new XResult<AllotAmountRefundResponse>(null, ErrorCode.SUBMIT_REPEAT);
            }

            try
            {
                if (!_lockProvider.Lock(requestHash))
                {
                    return new XResult<AllotAmountRefundResponse>(null, ErrorCode.SUBMIT_REPEAT);
                }

                //先找到同一提现单对应的分账成功的分帐单
                var existedAllotPaySuccessOrder = _allotAmountOrderRepository.QueryProvider.FirstOrDefault(x => x.WithdrawOutTradeNo == request.WithdrawOutTradeNo
                && x.Status == AllotAmountOrderStatus.SUCCESS.ToString()
                && x.AllotType == AllotAmountType.Pay.ToString());
                if (existedAllotPaySuccessOrder == null)
                {
                    return new XResult<AllotAmountRefundResponse>(null, ErrorCode.INFO_NOT_EXIST, new ArgumentException("消费分账单不存在"));
                }

                var existedOrder = _allotAmountOrderRepository.QueryProvider.FirstOrDefault(x => x.OriginalOutTradeNo == existedAllotPaySuccessOrder.OutTradeNo
                && x.Status != AllotAmountOrderStatus.SUCCESS.ToString()
                && x.Status != AllotAmountOrderStatus.FAILURE.ToString()
                && x.AllotType == AllotAmountType.Refund.ToString());

                if (existedOrder != null)
                {
                    return new XResult<AllotAmountRefundResponse>(null, ErrorCode.INFO_EXISTED, new ArgumentException("退款分账单已存在"));
                }

                var newId = IDGenerator.GenerateID();
                existedOrder = new AllotAmountOrder()
                {
                    Id = newId,
                    AppId = request.AppId,
                    TradeNo = newId.ToString(),
                    PayeeId = request.PayeeId,
                    WithdrawOutTradeNo = request.WithdrawOutTradeNo,
                    OutTradeNo = newId.ToString(),
                    OriginalOutTradeNo = existedAllotPaySuccessOrder.OutTradeNo,
                    TotalAmount = request.TotalAmount,
                    AllotType = AllotAmountType.Refund.ToString(),
                    SettlePeriod = request.SettlePeriod,
                    Status = AllotAmountOrderStatus.APPLY.ToString(),
                    ApplyTime = DateTime.Now
                };

                _allotAmountOrderRepository.Add(existedOrder);
                var saveResult = _allotAmountOrderRepository.SaveChanges();
                if (!saveResult.Success)
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(_allotAmountOrderRepository)}.SaveChanges()", "保存分账订单失败", saveResult.FirstException, existedOrder);
                    return new XResult<AllotAmountRefundResponse>(null, ErrorCode.DB_UPDATE_FAILED, new DbUpdateException("保存分账订单失败"));
                }

                String traceMethod = $"Bill99Util.Execute(/settle/refund)";

                _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.OK.ToString(), service, traceMethod, LogPhase.BEGIN);

                var settleData = new RawSettleData[1];
                settleData[0] = new RawSettleData()
                {
                    amount = request.TotalAmount,
                    merchantUid = request.PayeeId,
                    outSubOrderNo = existedOrder.OutTradeNo,
                    origOutSubOrderNo = existedOrder.OutTradeNo,
                    settlePeriod = request.SettlePeriod
                };

                var execResult = Bill99Util.Execute<RawAllotAmountRefundRequest, RawAllotAmountRefundResponse>("/settle/refund", new RawAllotAmountRefundRequest()
                {
                    outOrderNo = existedOrder.OutTradeNo,
                    origOutOrderNo = existedOrder.OriginalOutTradeNo,
                    settleData = settleData,
                    totalAmount = request.TotalAmount
                });

                _logger.Trace(TraceType.BLL.ToString(), (execResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), service, traceMethod, LogPhase.END);

                if (!execResult.Success)
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, traceMethod, "申请退款分账失败", execResult.FirstException, request);
                    return new XResult<AllotAmountRefundResponse>(null, ErrorCode.DEPENDENT_API_CALL_FAILED, execResult.FirstException);
                }

                if (execResult.Value.ResponseCode != "0000")
                {
                    _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, traceMethod, LogPhase.ACTION, "申请退款分账失败", new Object[] { request, execResult.Value });
                    return new XResult<AllotAmountRefundResponse>(null, ErrorCode.DEPENDENT_API_CALL_FAILED, new RemoteException($"{execResult.Value.ResponseCode}:{execResult.Value.ResponseMessage}"));
                }

                existedOrder.Status = AllotAmountOrderStatus.PROCESSING.ToString();
                _allotAmountOrderRepository.Update(existedOrder);

                var updateResult = _allotAmountOrderRepository.SaveChanges();
                if (!updateResult.Success)
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(_allotAmountOrderRepository)}.SaveChanges()", "更新分账结果失败", updateResult.FirstException, existedOrder);
                }

                return new XResult<AllotAmountRefundResponse>(new AllotAmountRefundResponse()
                {
                    Status = existedOrder.Status,
                    Msg = GetAllotAmountOrderStatusMsg(existedOrder.Status)
                });
            }
            finally
            {
                _lockProvider.UnLock(requestHash);
            }
        }

        public XResult<SettlementPeriodModifyResponse> ModifySettlePeriod(SettlementPeriodModifyRequest request)
        {
            return Bill99Util.Execute<SettlementPeriodModifyRequest, SettlementPeriodModifyResponse>("/settle/period/modify", request);
        }

        private String GetAllotAmountOrderStatusMsg(String status)
        {
            switch (status)
            {
                case "APPLY":
                    return AllotAmountOrderStatus.APPLY.GetDescription();
                case "PROCESSING":
                    return AllotAmountOrderStatus.PROCESSING.GetDescription();
                case "SUCCESS":
                    return AllotAmountOrderStatus.SUCCESS.GetDescription();
                case "FAILURE":
                    return AllotAmountOrderStatus.FAILURE.GetDescription();
            }

            return String.Empty;
        }
    }
}
