using System;
using System.Collections.Generic;
using System.Text;
using CPI.Common;
using CPI.Common.Domain.SettleDomain.Bill99;
using CPI.Common.Domain.SettleDomain.Bill99.v1_0;
using CPI.IService.SettleServices;
using CPI.Utils;
using Lotus.Core;
using Lotus.Logging;

namespace CPI.Handlers.Settle
{
    internal class Bill99PersonalInvocation : IInvocation
    {
        private static readonly ILogger _logger = LogManager.GetLogger();

        private readonly GatewayCommonRequest _request;
        private readonly IPersonalService _service;
        private readonly IPersonalServiceV1 _serviceV1;

        public Bill99PersonalInvocation(GatewayCommonRequest request)
        {
            _request = request;
            _service = XDI.Resolve<IPersonalService>();
            _serviceV1 = XDI.Resolve<IPersonalServiceV1>();
        }

        public ObjectResult Invoke()
        {
            String traceService = $"{this.GetType().FullName}.Invoke()";
            String requestService = $"{_request.Method}.{_request.Version}";
            String traceMethod = String.Empty;

            switch (requestService)
            {
                case "cpi.settle.personal.register.1.0":
                    return Register_1_0(traceService, requestService, ref traceMethod);
                case "cpi.settle.personal.register.1.1":
                    return Register_1_1(traceService, requestService, ref traceMethod);
                case "cpi.settle.personal.updateinfo.1.0":
                    return UpdateInfo_1_0(traceService, requestService, ref traceMethod);
                case "cpi.settle.personal.info.1.0":
                    return QueryInfo_1_0(traceService, requestService, ref traceMethod);
                case "cpi.settle.personal.bindcard.1.0":
                    return BindCard_1_0(traceService, requestService, ref traceMethod);
                case "cpi.settle.personal.bindcard.1.1":
                    return BindCard_1_1(traceService, requestService, ref traceMethod);
                case "cpi.settle.personal.acceptbankcard.1.1":
                    return AcceptBankCard_1_1(traceService, requestService, ref traceMethod);
                case "cpi.settle.personal.applybindcard.1.1":
                    return ApplyBindCard_1_1(traceService, requestService, ref traceMethod);
                case "cpi.settle.personal.rebindcard.1.0":
                    return ReBindCard_1_0(traceService, requestService, ref traceMethod);
                case "cpi.settle.personal.cancelbindcard.1.0":
                    return CancelBindCard_1_0(traceService, requestService, ref traceMethod);
                case "cpi.settle.personal.bindcardlist.1.0":
                    return BindCardList_1_0(traceService, requestService, ref traceMethod);
                case "cpi.settle.personal.bindcard.querystatus.1.0":
                    return QueryStatus_1_0(traceService, requestService, ref traceMethod);
            }

            return new ObjectResult(null, ErrorCode.METHOD_NOT_SUPPORT, new NotSupportedException($"method \"{ _request.Method }\" not support"));
        }

        private ObjectResult QueryStatus_1_0(String traceService, String requestService, ref String traceMethod)
        {
            var queryStatusRequest = JsonUtil.DeserializeObject<WithdrawBindCardQueryStatusRequest>(_request.BizContent);
            if (!queryStatusRequest.Success)
            {
                _logger.Error(TraceType.ROUTE.ToString(), CallResultStatus.ERROR.ToString(), traceService, requestService, "BizContent解析失败", queryStatusRequest.FirstException, _request.BizContent);
                return new ObjectResult(null, ErrorCode.BIZ_CONTENT_DESERIALIZE_FAILED);
            }
            queryStatusRequest.Value.AppId = _request.AppId;
            traceMethod = $"{_service.GetType().FullName}.QueryBindCardStatus(...)";
            _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), traceService, traceMethod, LogPhase.BEGIN, "开始查询个人绑卡状态", queryStatusRequest.Value);

            var queryStatusResult = _service.QueryBindCardStatus(queryStatusRequest.Value);
            _logger.Trace(TraceType.ROUTE.ToString(), (queryStatusResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), traceService, traceMethod, LogPhase.END, "结束查询个人绑卡状态", queryStatusResult.Value);

            return queryStatusResult.Success ? new ObjectResult(queryStatusResult.Value) : new ObjectResult(null, queryStatusResult.ErrorCode, queryStatusResult.FirstException);
        }

        private ObjectResult BindCardList_1_0(String traceService, String requestService, ref String traceMethod)
        {
            var queryListRequest = JsonUtil.DeserializeObject<PersonalBoundCardListQueryRequest>(_request.BizContent);
            if (!queryListRequest.Success)
            {
                _logger.Error(TraceType.ROUTE.ToString(), CallResultStatus.ERROR.ToString(), traceService, requestService, "BizContent解析失败", queryListRequest.FirstException, _request.BizContent);
                return new ObjectResult(null, ErrorCode.BIZ_CONTENT_DESERIALIZE_FAILED);
            }
            queryListRequest.Value.AppId = _request.AppId;

            traceMethod = $"{_service.GetType().FullName}.GetBoundCards(...)";
            _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), traceService, traceMethod, LogPhase.BEGIN, "开始查询个人绑卡列表", queryListRequest.Value);

            var queryListResult = _service.GetBoundCards(queryListRequest.Value);

            _logger.Trace(TraceType.ROUTE.ToString(), (queryListResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), traceService, traceMethod, LogPhase.END, "结束查询个人绑卡列表", queryListResult.Value);

            return queryListResult.Success ? new ObjectResult(queryListResult.Value) : new ObjectResult(null, queryListResult.ErrorCode, queryListResult.FirstException);
        }

        private ObjectResult CancelBindCard_1_0(String traceService, String requestService, ref String traceMethod)
        {
            var cancelRequest = JsonUtil.DeserializeObject<PersonalCancelBoundCardRequest>(_request.BizContent);
            if (!cancelRequest.Success)
            {
                _logger.Error(TraceType.ROUTE.ToString(), CallResultStatus.ERROR.ToString(), traceService, requestService, "BizContent解析失败", cancelRequest.FirstException, _request.BizContent);
                return new ObjectResult(null, ErrorCode.BIZ_CONTENT_DESERIALIZE_FAILED);
            }
            cancelRequest.Value.AppId = _request.AppId;

            traceMethod = $"{_service.GetType().FullName}.CancelBoundCard(...)";
            _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), traceService, traceMethod, LogPhase.BEGIN, "开始取消个人绑卡", cancelRequest.Value);

            var cancelResult = _service.CancelBoundCard(cancelRequest.Value);

            _logger.Trace(TraceType.ROUTE.ToString(), (cancelResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), traceService, traceMethod, LogPhase.END, "结束取消个人绑卡", cancelResult.Value);

            return cancelResult.Success ? new ObjectResult(cancelResult.Value) : new ObjectResult(null, cancelResult.ErrorCode, cancelResult.FirstException);
        }

        private ObjectResult ReBindCard_1_0(String traceService, String requestService, ref String traceMethod)
        {
            var rebindRequest = JsonUtil.DeserializeObject<PersonalWithdrawRebindCardRequest>(_request.BizContent);
            if (!rebindRequest.Success)
            {
                _logger.Error(TraceType.ROUTE.ToString(), CallResultStatus.ERROR.ToString(), traceService, requestService, "BizContent解析失败", rebindRequest.FirstException, _request.BizContent);
                return new ObjectResult(null, ErrorCode.BIZ_CONTENT_DESERIALIZE_FAILED);
            }
            rebindRequest.Value.AppId = _request.AppId;

            traceMethod = $"{_service.GetType().FullName}.WithdrawRebindCard(...)";
            _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), traceService, traceMethod, LogPhase.BEGIN, "开始个人重新绑卡", rebindRequest.Value);

            var rebindResult = _service.WithdrawRebindCard(rebindRequest.Value);

            _logger.Trace(TraceType.ROUTE.ToString(), (rebindResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), traceService, traceMethod, LogPhase.END, "结束个人重新绑卡", rebindResult.Value);

            return rebindResult.Success ? new ObjectResult(rebindResult.Value) : new ObjectResult(null, rebindResult.ErrorCode, rebindResult.FirstException);
        }

        private ObjectResult BindCard_1_0(String traceService, String requestService, ref String traceMethod)
        {
            var bindRequest = JsonUtil.DeserializeObject<PersonalWithdrawBindCardRequest>(_request.BizContent);
            if (!bindRequest.Success)
            {
                _logger.Error(TraceType.ROUTE.ToString(), CallResultStatus.ERROR.ToString(), traceService, requestService, "BizContent解析失败", bindRequest.FirstException, _request.BizContent);
                return new ObjectResult(null, ErrorCode.BIZ_CONTENT_DESERIALIZE_FAILED);
            }
            bindRequest.Value.AppId = _request.AppId;

            traceMethod = $"{_service.GetType().FullName}.WithdrawBindCard(...)";
            _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), traceService, traceMethod, LogPhase.BEGIN, "开始个人提现绑卡", bindRequest.Value);

            var bindResult = _service.WithdrawBindCard(bindRequest.Value);

            _logger.Trace(TraceType.ROUTE.ToString(), (bindResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), traceService, traceMethod, LogPhase.END, "结束个人提现绑卡", bindResult.Value);

            return bindResult.Success ? new ObjectResult(bindResult.Value) : new ObjectResult(null, bindResult.ErrorCode, bindResult.FirstException);
        }

        private ObjectResult BindCard_1_1(String traceService, String requestService, ref String traceMethod)
        {
            var bindRequest = JsonUtil.DeserializeObject<PersonalWithdrawBindCardRequestV1>(_request.BizContent);
            if (!bindRequest.Success)
            {
                _logger.Error(TraceType.ROUTE.ToString(), CallResultStatus.ERROR.ToString(), traceService, requestService, "BizContent解析失败", bindRequest.FirstException, _request.BizContent);
                return new ObjectResult(null, ErrorCode.BIZ_CONTENT_DESERIALIZE_FAILED);
            }
            bindRequest.Value.AppId = _request.AppId;

            traceMethod = $"{_serviceV1.GetType().FullName}.WithdrawBindCard_1_1(...)";
            _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), traceService, traceMethod, LogPhase.BEGIN, "开始个人提现绑卡", bindRequest.Value);

            var bindResult = _serviceV1.WithdrawBindCard(bindRequest.Value);

            _logger.Trace(TraceType.ROUTE.ToString(), (bindResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), traceService, traceMethod, LogPhase.END, "结束个人提现绑卡", bindResult.Value);

            return bindResult.Success ? new ObjectResult(bindResult.Value) : new ObjectResult(null, bindResult.ErrorCode, bindResult.FirstException);
        }

        private ObjectResult QueryInfo_1_0(String traceService, String requestService, ref String traceMethod)
        {
            var queryInfoRequest = JsonUtil.DeserializeObject<PersonalInfoQueryRequest>(_request.BizContent);
            if (!queryInfoRequest.Success)
            {
                _logger.Error(TraceType.ROUTE.ToString(), CallResultStatus.ERROR.ToString(), traceService, requestService, "BizContent解析失败", queryInfoRequest.FirstException, _request.BizContent);
                return new ObjectResult(null, ErrorCode.BIZ_CONTENT_DESERIALIZE_FAILED);
            }
            queryInfoRequest.Value.AppId = _request.AppId;

            traceMethod = $"{_service.GetType().FullName}.GetAccountInfo(...)";
            _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), traceService, traceMethod, LogPhase.BEGIN, "开始查询个人账户信息", queryInfoRequest.Value);

            var queryInfoResult = _service.GetAccountInfo(queryInfoRequest.Value);

            _logger.Trace(TraceType.ROUTE.ToString(), (queryInfoResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), traceService, traceMethod, LogPhase.END, "结束查询个人账户信息", queryInfoResult.Value);

            return queryInfoResult.Success ? new ObjectResult(queryInfoResult.Value) : new ObjectResult(null, queryInfoResult.ErrorCode, queryInfoResult.FirstException);
        }

        private ObjectResult UpdateInfo_1_0(String traceService, String requestService, ref String traceMethod)
        {
            var updateRequest = JsonUtil.DeserializeObject<PersonalInfoUpdateRequest>(_request.BizContent);
            if (!updateRequest.Success)
            {
                _logger.Error(TraceType.ROUTE.ToString(), CallResultStatus.ERROR.ToString(), traceService, requestService, "BizContent解析失败", updateRequest.FirstException, _request.BizContent);
                return new ObjectResult(null, ErrorCode.BIZ_CONTENT_DESERIALIZE_FAILED);
            }
            updateRequest.Value.AppId = _request.AppId;

            traceMethod = $"{_service.GetType().FullName}.UpdateAccountInfo(...)";
            _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), traceService, traceMethod, LogPhase.BEGIN, "开始更新个人账户信息", updateRequest.Value);

            var updateResult = _service.UpdateAccountInfo(updateRequest.Value);

            _logger.Trace(TraceType.ROUTE.ToString(), (updateResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), traceService, traceMethod, LogPhase.END, "结束更新个人账户信息", updateResult.Value);

            return updateResult.Success ? new ObjectResult(updateResult.Value) : new ObjectResult(null, updateResult.ErrorCode, updateResult.FirstException);
        }

        private ObjectResult Register_1_0(String traceService, String requestService, ref String traceMethod)
        {
            var regRequest = JsonUtil.DeserializeObject<PersonalRegisterRequest>(_request.BizContent);
            if (!regRequest.Success)
            {
                _logger.Error(TraceType.ROUTE.ToString(), CallResultStatus.ERROR.ToString(), traceService, requestService, "BizContent解析失败", regRequest.FirstException, _request.BizContent);
                return new ObjectResult(null, ErrorCode.BIZ_CONTENT_DESERIALIZE_FAILED);
            }
            regRequest.Value.AppId = _request.AppId;

            traceMethod = $"{_service.GetType().FullName}.Register(...)";
            _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), traceService, traceMethod, LogPhase.BEGIN, "开始个人开户", regRequest.Value);

            var regResult = _service.Register(regRequest.Value);

            _logger.Trace(TraceType.ROUTE.ToString(), (regResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), traceService, traceMethod, LogPhase.END, "结束个人开户", regResult.Value);

            return regResult.Success ? new ObjectResult(regResult.Value) : new ObjectResult(null, regResult.ErrorCode, regResult.FirstException);
        }

        private ObjectResult Register_1_1(String traceService, String requestService, ref String traceMethod)
        {
            var regRequest = JsonUtil.DeserializeObject<PersonalRegisterRequestV1>(_request.BizContent);
            if (!regRequest.Success)
            {
                _logger.Error(TraceType.ROUTE.ToString(), CallResultStatus.ERROR.ToString(), traceService, requestService, "BizContent解析失败", regRequest.FirstException, _request.BizContent);
                return new ObjectResult(null, ErrorCode.BIZ_CONTENT_DESERIALIZE_FAILED);
            }
            regRequest.Value.AppId = _request.AppId;

            traceMethod = $"{_serviceV1.GetType().FullName}.Register(...)";
            _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), traceService, traceMethod, LogPhase.BEGIN, "开始个人开户", regRequest.Value);

            var regResult = _serviceV1.Register(regRequest.Value);

            _logger.Trace(TraceType.ROUTE.ToString(), (regResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), traceService, traceMethod, LogPhase.END, "结束个人开户", regResult.Value);

            return regResult.Success ? new ObjectResult(regResult.Value) : new ObjectResult(null, regResult.ErrorCode, regResult.FirstException);
        }

        private ObjectResult AcceptBankCard_1_1(String traceService, String requestService, ref String traceMethod)
        {
            var request = JsonUtil.DeserializeObject<QueryBankCardAcceptRequestV1>(_request.BizContent);
            if (!request.Success)
            {
                _logger.Error(TraceType.ROUTE.ToString(), CallResultStatus.ERROR.ToString(), traceService, requestService, "BizContent解析失败", request.FirstException, _request.BizContent);
                return new ObjectResult(null, ErrorCode.BIZ_CONTENT_DESERIALIZE_FAILED);
            }
            request.Value.AppId = _request.AppId;

            traceMethod = $"{_serviceV1.GetType().FullName}.QueryBankCardAccept(...)";
            _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), traceService, traceMethod, LogPhase.BEGIN, "开始调用查询银行卡受理能力接口", request.Value);

            var acceptResult = _serviceV1.QueryBankCardAccept(request.Value);

            _logger.Trace(TraceType.ROUTE.ToString(), (acceptResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), traceService, traceMethod, LogPhase.END, "结束调用查询银行卡受理能力接口", acceptResult.Value);

            return acceptResult.Success ? new ObjectResult(acceptResult.Value) : new ObjectResult(null, acceptResult.ErrorCode, acceptResult.FirstException);
        }

        private ObjectResult ApplyBindCard_1_1(String traceService, String requestService, ref String traceMethod)
        {
            var applyRequest = JsonUtil.DeserializeObject<PersonalApplyBindCardRequestV1>(_request.BizContent);
            if (!applyRequest.Success)
            {
                _logger.Error(TraceType.ROUTE.ToString(), CallResultStatus.ERROR.ToString(), traceService, requestService, "BizContent解析失败", applyRequest.FirstException, _request.BizContent);
                return new ObjectResult(null, ErrorCode.BIZ_CONTENT_DESERIALIZE_FAILED);
            }
            applyRequest.Value.AppId = _request.AppId;

            traceMethod = $"{_service.GetType().FullName}.ApplyBindCard_1_1(...)";
            _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), traceService, traceMethod, LogPhase.BEGIN, "开始申请提现绑卡", applyRequest.Value);

            var applyResult = _serviceV1.ApplyBindCard(applyRequest.Value);

            _logger.Trace(TraceType.ROUTE.ToString(), (applyResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), traceService, traceMethod, LogPhase.END, "结束申请提现绑卡", applyResult.Value);

            return applyResult.Success ? new ObjectResult(applyResult.Value) : new ObjectResult(null, applyResult.ErrorCode, applyResult.FirstException);
        }
    }
}
