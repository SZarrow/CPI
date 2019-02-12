using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CPI.Common;
using CPI.Common.Domain.SettleDomain.Bill99;
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
    public class AllotAmountWithdrawService : IAllotAmountWithdrawService
    {
        private static readonly LockProvider _lockProvider = new LockProvider();
        private static readonly ILogger _logger = LogManager.GetLogger();

        private readonly IAllotAmountWithdrawOrderRepository _allotAmountWithdrawOrderRepository = null;
        private readonly IAllotAmountOrderRepository _allotAmountOrderRepository = null;
        private readonly IPersonalSubAccountRepository _personalSubAccountRepository = null;
        private readonly IWithdrawBankCardBindInfoRepository _withdrawBankCardBindInfoRepository = null;

        private readonly IAllotAmountService _allotAmountService = null;
        //private readonly IWithdrawService _withdrawService = null;

        public XResult<AllotAmountWithdrawApplyResponse> Apply(AllotAmountWithdrawApplyRequest request)
        {
            if (request == null)
            {
                return new XResult<AllotAmountWithdrawApplyResponse>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentNullException(nameof(request)));
            }

            String service = $"{this.GetType().FullName}.Apply(...)";

            if (!request.IsValid)
            {
                _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(request)}.IsValid", LogPhase.ACTION, $"快钱盈帐通：{request.ErrorMessage}", request);
                return new XResult<AllotAmountWithdrawApplyResponse>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentException(request.ErrorMessage));
            }

            if (request.Amount < GlobalConfig.X99bill_YZT_WithdrawMinAmount)
            {
                return new XResult<AllotAmountWithdrawApplyResponse>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentException($"提现金额至少为{GlobalConfig.X99bill_YZT_WithdrawMinAmount.ToString()}元"));
            }

            var requestHash = $"withdraw:{request.PayeeId}".GetHashCode();

            if (_lockProvider.Exists(requestHash))
            {
                return new XResult<AllotAmountWithdrawApplyResponse>(null, ErrorCode.SUBMIT_REPEAT);
            }

            try
            {
                if (!_lockProvider.Lock(requestHash))
                {
                    return new XResult<AllotAmountWithdrawApplyResponse>(null, ErrorCode.SUBMIT_REPEAT);
                }

                //检查是否已开户
                if (!_personalSubAccountRepository.Exists(x => x.AppId == request.AppId && x.UID == request.PayeeId))
                {
                    _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, "__CheckPersonalAccountRegisterInfo", LogPhase.ACTION, "该用户尚未开户", request);
                    return new XResult<AllotAmountWithdrawApplyResponse>(null, SettleErrorCode.UN_REGISTERED);
                }

                //检查是否已绑卡
                if (!_withdrawBankCardBindInfoRepository.Exists(x => x.AppId == request.AppId && x.PayeeId == request.PayeeId))
                {
                    _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, "__CheckBankCardBindInfo", LogPhase.ACTION, "该用户尚未绑卡", request);
                    return new XResult<AllotAmountWithdrawApplyResponse>(null, SettleErrorCode.NO_BANKCARD_BOUND);
                }

                var existsOrder = _allotAmountWithdrawOrderRepository.Exists(x => x.OutTradeNo == request.OutTradeNo);
                if (existsOrder)
                {
                    return new XResult<AllotAmountWithdrawApplyResponse>(null, ErrorCode.OUT_TRADE_NO_EXISTED);
                }

                var newId = IDGenerator.GenerateID();
                var newWithdrawOrder = new AllotAmountWithdrawOrder()
                {
                    Id = newId,
                    AppId = request.AppId,
                    TradeNo = newId.ToString(),
                    PayeeId = request.PayeeId,
                    OutTradeNo = request.OutTradeNo,
                    Amount = request.Amount,
                    CustomerFee = request.CustomerFee,
                    MerchantFee = request.MerchantFee,
                    SettlePeriod = request.SettlePeriod,
                    Status = WithdrawOrderStatus.APPLY.ToString(),
                    ApplyTime = DateTime.Now
                };

                _allotAmountWithdrawOrderRepository.Add(newWithdrawOrder);

                var saveResult = _allotAmountWithdrawOrderRepository.SaveChanges();
                if (!saveResult.Success)
                {
                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(_allotAmountWithdrawOrderRepository)}.SaveChanges()", "保存分账提现记录失败", saveResult.FirstException, newWithdrawOrder);
                    return new XResult<AllotAmountWithdrawApplyResponse>(null, ErrorCode.DB_UPDATE_FAILED, saveResult.FirstException);
                }

                var resp = new AllotAmountWithdrawApplyResponse()
                {
                    PayeeId = newWithdrawOrder.PayeeId,
                    OutTradeNo = newWithdrawOrder.OutTradeNo,
                    Amount = newWithdrawOrder.Amount,
                    Status = CommonStatus.SUCCESS.ToString(),
                    Msg = $"申请{CommonStatus.SUCCESS.GetDescription()}"
                };

                return new XResult<AllotAmountWithdrawApplyResponse>(resp);
            }
            finally
            {
                _lockProvider.UnLock(requestHash);
            }
        }

        public XResult<Int32> FireAllotAmount(Int32 count = 10)
        {
            if (count <= 0)
            {
                return new XResult<Int32>(0);
            }

            String service = $"{this.GetType().FullName}.FireAllotAmount()";

            var hashKey = $"fireallotamount:{DateTime.Now.ToString("yyMMddHH")}".GetHashCode();

            if (_lockProvider.Exists(hashKey))
            {
                return new XResult<Int32>(0, ErrorCode.SUBMIT_REPEAT);
            }

            try
            {
                if (!_lockProvider.Lock(hashKey))
                {
                    return new XResult<Int32>(0, ErrorCode.SUBMIT_REPEAT);
                }

                //消费分账的提现单
                var withdrawOrders = (from t0 in _allotAmountWithdrawOrderRepository.QueryProvider
                                      where t0.Status == WithdrawOrderStatus.APPLY.ToString()
                                      orderby t0.ApplyTime
                                      select t0).Take(count).ToList();

                if (withdrawOrders == null || withdrawOrders.Count == 0)
                {
                    return new XResult<Int32>(0);
                }

                var successCount = 0;

                foreach (var withdrawOrder in withdrawOrders)
                {
                    var result = _allotAmountService.Pay(new AllotAmountPayRequest()
                    {
                        AppId = withdrawOrder.AppId,
                        PayeeId = withdrawOrder.PayeeId,
                        TotalAmount = withdrawOrder.Amount,
                        SettlePeriod = withdrawOrder.SettlePeriod,
                        WithdrawOutTradeNo = withdrawOrder.OutTradeNo
                    });

                    if (result.Success)
                    {
                        successCount++;
                    }
                }

                return new XResult<Int32>(successCount);
            }
            finally
            {
                _lockProvider.UnLock(hashKey);
            }
        }

        public XResult<Int32> PullAllotAmountResult(Int32 count = 20)
        {
            if (count <= 0)
            {
                return new XResult<Int32>(0);
            }

            String service = $"{this.GetType().FullName}.PullAllotAmountResult()";

            var hashKey = $"pullallotamount:{DateTime.Now.ToString("yyMMddHH")}".GetHashCode();

            if (_lockProvider.Exists(hashKey))
            {
                return new XResult<Int32>(0, ErrorCode.SUBMIT_REPEAT);
            }

            try
            {
                if (!_lockProvider.Lock(hashKey))
                {
                    return new XResult<Int32>(0, ErrorCode.SUBMIT_REPEAT);
                }

                var allotAmountOrders = (from t0 in _allotAmountOrderRepository.QueryProvider
                                         where t0.Status == AllotAmountOrderStatus.PROCESSING.ToString()
                                         orderby t0.ApplyTime
                                         select t0).Take(count).ToList();

                if (allotAmountOrders == null || allotAmountOrders.Count == 0)
                {
                    return new XResult<Int32>(0);
                }

                var c_allotAmountOrders = new Stack<AllotAmountOrder>(allotAmountOrders);
                var successCount = 0;

                while (c_allotAmountOrders.Count > 0)
                {
                    var allotAmountOrder = c_allotAmountOrders.Pop();

                    var request = new AllotAmountResultQueryRequest()
                    {
                        AppId = allotAmountOrder.AppId,
                        OutOrderNo = allotAmountOrder.OutTradeNo
                    };

                    String traceMethod = $"Bill99Util.Execute(/settle/detail)";

                    _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.OK.ToString(), service, traceMethod, LogPhase.BEGIN);

                    var respResult = Bill99UtilYZT.Execute<AllotAmountResultQueryRequest, AllotAmountResultQueryResponse>("/settle/detail", request);

                    _logger.Trace(TraceType.BLL.ToString(), (respResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), service, traceMethod, LogPhase.BEGIN);

                    if (!respResult.Success || respResult.Value == null)
                    {
                        _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, "Bill99Util.Execute(...)", "快钱盈帐通：查询分账结果失败", respResult.FirstException, request);
                        continue;
                    }

                    if (respResult.Value.ResponseCode == "0000")
                    {
                        if (respResult.Value.SettleResults == null || respResult.Value.SettleResults.Count() == 0)
                        {
                            _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, "respResult.Value.SettleResults", "快钱盈帐通：查询成功但第三方返回的分账结果为空", null, respResult.Value);
                        }
                        else
                        {
                            var settleResult = respResult.Value.SettleResults.FirstOrDefault();
                            Boolean statusHasChanged = false;
                            switch (settleResult.SettleStatus)
                            {
                                case "9":
                                    allotAmountOrder.Status = AllotAmountOrderStatus.SUCCESS.ToString();
                                    statusHasChanged = true;
                                    break;
                                case "8":
                                    allotAmountOrder.Status = AllotAmountOrderStatus.FAILURE.ToString();
                                    statusHasChanged = true;
                                    break;
                            }

                            if (statusHasChanged)
                            {
                                var updateResult = _allotAmountOrderRepository.SaveChanges();
                                if (!updateResult.Success)
                                {
                                    _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(_allotAmountOrderRepository)}.SaveChanges()", "快钱盈帐通：更新分账结果状态失败", updateResult.FirstException, allotAmountOrder);
                                    continue;
                                }
                            }

                            successCount++;
                        }
                    }
                }

                return new XResult<Int32>(successCount);
            }
            finally
            {
                _lockProvider.UnLock(hashKey);
            }
        }

        public XResult<Int32> FireWithdraw(Int32 count = 10)
        {
            //if (count <= 0)
            //{
            //    return new XResult<Int32>(0);
            //}

            //String service = $"{this.GetType().FullName}.FireWithdraw()";

            //var hashKey = $"firewithdraw:{DateTime.Now.ToString("yyMMddHH")}".GetHashCode();

            //if (_lockProvider.Exists(hashKey))
            //{
            //    return new XResult<Int32>(0, ErrorCode.SUBMIT_REPEAT);
            //}

            //try
            //{
            //    if (!_lockProvider.Lock(hashKey))
            //    {
            //        return new XResult<Int32>(0, ErrorCode.SUBMIT_REPEAT);
            //    }

            //    //查出分账成功的提现单
            //    var withdrawOrders = (from t0 in _allotAmountWithdrawOrderRepository.QueryProvider
            //                          join t1 in _allotAmountOrderRepository.QueryProvider
            //                          on t0.OutTradeNo equals t1.WithdrawOutTradeNo
            //                          where t0.Status == WithdrawOrderStatus.APPLY.ToString()
            //                          && t1.Status == AllotAmountOrderStatus.SUCCESS.ToString()
            //                          orderby t0.ApplyTime
            //                          select t0).Take(count).ToList();

            //    if (withdrawOrders == null || withdrawOrders.Count == 0)
            //    {
            //        return new XResult<Int32>(0);
            //    }

            //    var tasks = new List<Task>(withdrawOrders.Count);
            //    var successCount = 0;

            //    foreach (var withdrawOrder in withdrawOrders)
            //    {
            //        var result = _withdrawService.Withdraw(new WithdrawRequest()
            //        {
            //            AppId = withdrawOrder.AppId,
            //            PayeeId = withdrawOrder.PayeeId,
            //            OutTradeNo = withdrawOrder.OutTradeNo,
            //            Amount = withdrawOrder.Amount,
            //            CustomerFee = withdrawOrder.CustomerFee,
            //            MerchantFee = withdrawOrder.MerchantFee,
            //            SettlePeriod = withdrawOrder.SettlePeriod
            //        });

            //        if (result.Success)
            //        {
            //            successCount++;
            //        }
            //    }

            //    return new XResult<Int32>(successCount);
            //}
            //finally
            //{
            //    _lockProvider.UnLock(hashKey);
            //}

            throw new NotImplementedException();
        }

        public XResult<Int32> PullWithdrawResult(Int32 count = 20)
        {
            if (count <= 0)
            {
                return new XResult<Int32>(0);
            }

            String service = $"{this.GetType().FullName}.PullWithdrawResult()";

            var hashKey = $"pullwithdrawresult:{DateTime.Now.ToString("yyMMddHH")}".GetHashCode();

            if (_lockProvider.Exists(hashKey))
            {
                return new XResult<Int32>(0, ErrorCode.SUBMIT_REPEAT);
            }

            try
            {
                if (!_lockProvider.Lock(hashKey))
                {
                    return new XResult<Int32>(0, ErrorCode.SUBMIT_REPEAT);
                }

                var withdrawOrders = (from t0 in _allotAmountWithdrawOrderRepository.QueryProvider
                                      where t0.Status == WithdrawOrderStatus.PROCESSING.ToString()
                                      orderby t0.ApplyTime
                                      select t0).Take(count).ToList();

                if (withdrawOrders == null || withdrawOrders.Count == 0)
                {
                    return new XResult<Int32>(0);
                }

                var c_withdrawOrders = new Stack<AllotAmountWithdrawOrder>(withdrawOrders);
                var successCount = 0;

                while (c_withdrawOrders.Count > 0)
                {
                    var withdrawOrder = c_withdrawOrders.Pop();

                    var request = new RawWithdrawQueryRequest()
                    {
                        outTradeNo = withdrawOrder.OutTradeNo,
                        uId = withdrawOrder.PayeeId
                    };

                    String traceMethod = "Bill99Util.Execute(/withdraw/query)";

                    _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.OK.ToString(), service, traceMethod, LogPhase.BEGIN);

                    var respResult = Bill99UtilYZT.Execute<RawWithdrawQueryRequest, RawWithdrawQueryResponse>("/withdraw/query", request);

                    _logger.Trace(TraceType.BLL.ToString(), (respResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), service, traceMethod, LogPhase.END);

                    if (!respResult.Success || respResult.Value == null)
                    {
                        _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, traceMethod, "查询提现结果失败", respResult.FirstException, request);
                        continue;
                    }

                    if (respResult.Value.ResponseCode == "0000")
                    {
                        Boolean statusHasChanged = false;

                        switch (respResult.Value.Status)
                        {
                            case "1":
                                withdrawOrder.Status = WithdrawOrderStatus.SUCCESS.ToString();
                                statusHasChanged = true;
                                break;
                            case "2":
                                withdrawOrder.Status = WithdrawOrderStatus.FAILURE.ToString();
                                statusHasChanged = true;
                                break;
                        }

                        if (statusHasChanged)
                        {
                            var updateResult = _allotAmountWithdrawOrderRepository.SaveChanges();
                            if (!updateResult.Success)
                            {
                                _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(_allotAmountWithdrawOrderRepository)}.SaveChanges()", "更新提现结果状态失败", updateResult.FirstException, withdrawOrder);
                                continue;
                            }
                        }

                        successCount++;
                    }
                }

                return new XResult<Int32>(successCount);
            }
            finally
            {
                _lockProvider.UnLock(hashKey);
            }
        }
    }
}
