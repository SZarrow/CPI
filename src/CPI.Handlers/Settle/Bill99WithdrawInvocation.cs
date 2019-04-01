using System;
using System.Collections.Generic;
using System.Text;
using CPI.Common;
using CPI.Common.Domain.Common;
using CPI.Common.Domain.SettleDomain.Bill99;
using CPI.IService.SettleServices;
using CPI.Utils;
using ATBase.Core;
using ATBase.Logging;

namespace CPI.Handlers.Settle
{
    internal class Bill99WithdrawInvocation : IInvocation
    {
        private static readonly ILogger _logger = LogManager.GetLogger();

        private readonly GatewayCommonRequest _request;
        private readonly IWithdrawService _withdrawService;
        private readonly IAllotAmountWithdrawService _allotAmountWithdrawService;

        public Bill99WithdrawInvocation(GatewayCommonRequest request)
        {
            _request = request;
            _withdrawService = XDI.Resolve<IWithdrawService>();
            _allotAmountWithdrawService = XDI.Resolve<IAllotAmountWithdrawService>();
        }

        public ObjectResult Invoke()
        {
            String traceService = $"{this.GetType().FullName}.Invoke()";
            String requestService = $"{_request.Method}.{_request.Version}";
            String traceMethod = String.Empty;

            switch (requestService)
            {
                case "cpi.settle.allotamount.withdraw.apply.1.0":
                    var commonWithdrawRequest = JsonUtil.DeserializeObject<CommonWithdrawRequest>(_request.BizContent);
                    if (!commonWithdrawRequest.Success)
                    {
                        _logger.Error(TraceType.ROUTE.ToString(), CallResultStatus.ERROR.ToString(), traceService, requestService, "BizContent解析失败", commonWithdrawRequest.FirstException, _request.BizContent);
                        return new ObjectResult(null, ErrorCode.BIZ_CONTENT_DESERIALIZE_FAILED);
                    }

                    _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), traceService, "BuildWithdrawRequest(...)", LogPhase.BEGIN, "开始构造分账提现请求参数");
                    var withdrawApplyRequest = BuildWithdrawRequest(commonWithdrawRequest.Value);
                    _logger.Trace(TraceType.ROUTE.ToString(), (withdrawApplyRequest.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), traceService, "BuildWithdrawRequest(...)", LogPhase.END, "完成构造分账提现请求参数");

                    if (!withdrawApplyRequest.Success)
                    {
                        return new ObjectResult(null, withdrawApplyRequest.FirstException);
                    }


                    withdrawApplyRequest.Value.AppId = _request.AppId;

                    traceMethod = $"{_allotAmountWithdrawService.GetType().FullName}.Apply(...)";
                    _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), traceService, traceMethod, LogPhase.BEGIN, "开始申请分账提现", withdrawApplyRequest.Value);

                    var applyResult = _allotAmountWithdrawService.Apply(withdrawApplyRequest.Value);

                    _logger.Trace(TraceType.ROUTE.ToString(), (applyResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), traceService, traceMethod, LogPhase.END, "结束申请分账提现", applyResult.Value);

                    return applyResult.Success ? new ObjectResult(applyResult.Value) : new ObjectResult(null, applyResult.ErrorCode, applyResult.FirstException);
                case "cpi.settle.withdraw.pay.1.0":
                    var withdrawPayRequest = JsonUtil.DeserializeObject<WithdrawRequest>(_request.BizContent);
                    if (!withdrawPayRequest.Success)
                    {
                        _logger.Error(TraceType.ROUTE.ToString(), CallResultStatus.ERROR.ToString(), traceService, requestService, "BizContent解析失败", withdrawPayRequest.FirstException, _request.BizContent);
                        return new ObjectResult(null, ErrorCode.BIZ_CONTENT_DESERIALIZE_FAILED);
                    }
                    withdrawPayRequest.Value.AppId = _request.AppId;

                    traceMethod = $"{_withdrawService.GetType().FullName}.Withdraw(...)";
                    _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), traceService, traceMethod, LogPhase.BEGIN, "开始提现", withdrawPayRequest.Value);

                    var withdrawResult = _withdrawService.Withdraw(withdrawPayRequest.Value);

                    _logger.Trace(TraceType.ROUTE.ToString(), (withdrawResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), traceService, traceMethod, LogPhase.END, "结束提现", withdrawResult.Value);

                    return withdrawResult.Success ? new ObjectResult(withdrawResult.Value) : new ObjectResult(null, withdrawResult.ErrorCode, withdrawResult.FirstException);
                case "cpi.settle.withdraw.querydetails.1.0":
                    var queryDetailsRequest = JsonUtil.DeserializeObject<WithdrawQueryRequest>(_request.BizContent);
                    if (!queryDetailsRequest.Success)
                    {
                        _logger.Error(TraceType.ROUTE.ToString(), CallResultStatus.ERROR.ToString(), traceService, requestService, "BizContent解析失败", queryDetailsRequest.FirstException, _request.BizContent);
                        return new ObjectResult(null, ErrorCode.BIZ_CONTENT_DESERIALIZE_FAILED);
                    }
                    queryDetailsRequest.Value.AppId = _request.AppId;

                    traceMethod = $"{_withdrawService.GetType().FullName}.QueryDetails(...)";
                    _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), traceService, traceMethod, LogPhase.BEGIN, "开始查询提现结果详情", queryDetailsRequest.Value);

                    var queryDetailsResult = _withdrawService.QueryDetails(queryDetailsRequest.Value);

                    _logger.Trace(TraceType.ROUTE.ToString(), (queryDetailsResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), traceService, traceMethod, LogPhase.END, "结束查询提现结果详情", queryDetailsResult.Value);

                    return queryDetailsResult.Success ? new ObjectResult(queryDetailsResult.Value) : new ObjectResult(null, queryDetailsResult.ErrorCode, queryDetailsResult.FirstException);
                case "cpi.settle.withdraw.querystatus.1.0":
                    var queryStatusRequest = JsonUtil.DeserializeObject<WithdrawStatusQueryRequest>(_request.BizContent);
                    if (!queryStatusRequest.Success)
                    {
                        _logger.Error(TraceType.ROUTE.ToString(), CallResultStatus.ERROR.ToString(), traceService, requestService, "BizContent解析失败", queryStatusRequest.FirstException, _request.BizContent);
                        return new ObjectResult(null, ErrorCode.BIZ_CONTENT_DESERIALIZE_FAILED);
                    }
                    queryStatusRequest.Value.AppId = _request.AppId;

                    traceMethod = $"{_withdrawService.GetType().FullName}.QueryStatus(...)";
                    _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), traceService, traceMethod, LogPhase.BEGIN, "开始查询提现结果状态", queryStatusRequest.Value);

                    var queryStatusResult = _withdrawService.QueryStatus(queryStatusRequest.Value);

                    _logger.Trace(TraceType.ROUTE.ToString(), (queryStatusResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), traceService, traceMethod, LogPhase.END, "结束查询提现结果状态", queryStatusResult.Value);

                    return queryStatusResult.Success ? new ObjectResult(new PagedListResult<WithdrawStatusQueryResult>()
                    {
                        Items = queryStatusResult.Value,
                        PageIndex = queryStatusResult.Value.PageInfo.PageIndex,
                        PageSize = queryStatusResult.Value.PageInfo.PageSize,
                        TotalCount = queryStatusResult.Value.PageInfo.TotalCount
                    }) : new ObjectResult(null, queryStatusResult.ErrorCode, queryStatusResult.FirstException);
                case "cpi.settle.withdraw.queryfee.1.0":
                    var queryFeeRequest = JsonUtil.DeserializeObject<WithdrawQueryFeeRequest>(_request.BizContent);
                    if (!queryFeeRequest.Success)
                    {
                        _logger.Error(TraceType.ROUTE.ToString(), CallResultStatus.ERROR.ToString(), traceService, requestService, "BizContent解析失败", queryFeeRequest.FirstException, _request.BizContent);
                        return new ObjectResult(null, ErrorCode.BIZ_CONTENT_DESERIALIZE_FAILED);
                    }
                    queryFeeRequest.Value.AppId = _request.AppId;

                    traceMethod = $"{_withdrawService.GetType().FullName}.QueryFee(...)";
                    _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), traceService, traceMethod, LogPhase.BEGIN, "开始查询提现手续费", queryFeeRequest.Value);

                    var queryFeeResult = _withdrawService.QueryFee(queryFeeRequest.Value);

                    _logger.Trace(TraceType.ROUTE.ToString(), (queryFeeResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), traceService, traceMethod, LogPhase.END, "结束查询提现手续费", queryFeeResult.Value);

                    return queryFeeResult.Success ? new ObjectResult(queryFeeResult.Value) : new ObjectResult(null, queryFeeResult.ErrorCode, queryFeeResult.FirstException);
            }

            return new ObjectResult(null, ErrorCode.METHOD_NOT_SUPPORT, new NotSupportedException($"method \"{requestService}\" not support"));
        }

        private XResult<AllotAmountWithdrawApplyRequest> BuildWithdrawRequest(CommonWithdrawRequest request)
        {
            if (request == null)
            {
                return new XResult<AllotAmountWithdrawApplyRequest>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentNullException(nameof(request)));
            }

            if (!request.IsValid)
            {
                _logger.Error(TraceType.ROUTE.ToString(), CallResultStatus.ERROR.ToString(), $"{this.GetType().FullName}.BuildWithdrawRequest()", "构造分账提现请求参数", $"快钱盈帐通：参数验证失败：{request.ErrorMessage}");
                return new XResult<AllotAmountWithdrawApplyRequest>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentException(request.ErrorMessage));
            }

            var result = new AllotAmountWithdrawApplyRequest()
            {
                OutTradeNo = request.OutTradeNo,
                PayeeId = request.PayeeId,
                Amount = request.Amount,
                SettlePeriod = request.SettlePeriod,
                CustomerFee = 0,
                MerchantFee = 0
            };

            return new XResult<AllotAmountWithdrawApplyRequest>(result);
        }
    }
}
