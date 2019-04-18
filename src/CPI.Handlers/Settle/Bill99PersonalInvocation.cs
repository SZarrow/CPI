using System;
using System.Collections.Generic;
using System.Text;
using CPI.Common;
using CPI.Common.Domain.SettleDomain.Bill99;
using CPI.Common.Domain.SettleDomain.Bill99.v1_0;
using CPI.IService.SettleServices;
using CPI.Utils;
using ATBase.Core;
using ATBase.Logging;

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
            String traceService = $"{this.GetType().FullName}.{nameof(Invoke)}()";
            String requestService = $"{_request.Method}.{_request.Version}";
            String traceMethod = String.Empty;

            switch (requestService)
            {
                #region #v1.0
                //case "cpi.settle.personal.register.1.0":
                //    return Register_1_0(traceService, requestService, ref traceMethod);
                //case "cpi.settle.personal.updateinfo.1.0":
                //    return UpdateInfo_1_0(traceService, requestService, ref traceMethod);
                //case "cpi.settle.personal.info.1.0":
                //    return QueryInfo_1_0(traceService, requestService, ref traceMethod);
                //case "cpi.settle.personal.bindcard.1.0":
                //    return BindCard_1_0(traceService, requestService, ref traceMethod);
                //case "cpi.settle.personal.rebindcard.1.0":
                //    return ReBindCard_1_0(traceService, requestService, ref traceMethod);
                //case "cpi.settle.personal.cancelbindcard.1.0":
                //    return CancelBindCard_1_0(traceService, requestService, ref traceMethod);
                //case "cpi.settle.personal.bindcardlist.1.0":
                //    return BindCardList_1_0(traceService, requestService, ref traceMethod);
                //case "cpi.settle.personal.bindcard.querystatus.1.0":
                //    return QueryStatus_1_0(traceService, requestService, ref traceMethod);
                #endregion

                #region #v1.1
                case "cpi.settle.personal.register.1.1":
                    return Register_1_1(traceService, requestService, ref traceMethod);
                case "cpi.settle.personal.register.info.1.1":
                    return QueryRegisterInfo_1_1(traceService, requestService, ref traceMethod);
                case "cpi.settle.personal.account.list.1.1":
                    return QueryAccounts_1_1(traceService, requestService, ref traceMethod);
                case "cpi.settle.personal.contractinfo.1.1":
                    return ContractInfo_1_1(traceService, requestService, ref traceMethod);
                case "cpi.settle.personal.signcontract.1.1":
                    return SignContract_1_1(traceService, requestService, ref traceMethod);
                case "cpi.settle.personal.acceptbankcard.1.1":
                    return AcceptBankCard_1_1(traceService, requestService, ref traceMethod);
                case "cpi.settle.personal.applybindcard.1.1":
                    return ApplyBindCard_1_1(traceService, requestService, ref traceMethod);
                case "cpi.settle.personal.bindcard.1.1":
                    return BindCard_1_1(traceService, requestService, ref traceMethod);
                case "cpi.settle.personal.withdraw.1.1":
                    return Withdraw_1_1(traceService, requestService, ref traceMethod);
                case "cpi.settle.personal.accountbalance.1.1":
                    return AccountBalance_1_1(traceService, requestService, ref traceMethod);
                case "cpi.settle.personal.withdraw.query.1.1":
                    return QueryWithdrawOrder_1_1(traceService, requestService, ref traceMethod);
                case "cpi.settle.personal.withdraw.orders.query.1.1":
                    return QueryWithdrawOrderList_1_1(traceService, requestService, ref traceMethod);
                case "cpi.settle.personal.withdraw.pullresult.1.1":
                    return PayResultPull_1_1(traceService, requestService, ref traceMethod);
                    #endregion
            }

            return new ObjectResult(null, ErrorCode.METHOD_NOT_SUPPORT, new NotSupportedException($"不支持服务\"{requestService}\""));
        }

        #region #v1.0
        //private ObjectResult QueryStatus_1_0(String traceService, String requestService, ref String traceMethod)
        //{
        //    var queryStatusRequest = JsonUtil.DeserializeObject<WithdrawBindCardQueryStatusRequest>(_request.BizContent);
        //    if (!queryStatusRequest.Success)
        //    {
        //        _logger.Error(TraceType.ROUTE.ToString(), CallResultStatus.ERROR.ToString(), traceService, requestService, "BizContent解析失败", queryStatusRequest.FirstException, _request.BizContent);
        //        return new ObjectResult(null, ErrorCode.BIZ_CONTENT_DESERIALIZE_FAILED);
        //    }
        //    queryStatusRequest.Value.AppId = _request.AppId;
        //    traceMethod = $"{_service.GetType().FullName}.{nameof(_service.QueryBindCardStatus)}(...)";
        //    _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), traceService, traceMethod, LogPhase.BEGIN, "开始查询个人绑卡状态", queryStatusRequest.Value);

        //    var queryStatusResult = _service.QueryBindCardStatus(queryStatusRequest.Value);
        //    _logger.Trace(TraceType.ROUTE.ToString(), (queryStatusResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), traceService, traceMethod, LogPhase.END, "结束查询个人绑卡状态", queryStatusResult.Value);

        //    return queryStatusResult.Success ? new ObjectResult(queryStatusResult.Value) : new ObjectResult(null, queryStatusResult.ErrorCode, queryStatusResult.FirstException);
        //}

        //private ObjectResult BindCardList_1_0(String traceService, String requestService, ref String traceMethod)
        //{
        //    var queryListRequest = JsonUtil.DeserializeObject<PersonalBoundCardListQueryRequest>(_request.BizContent);
        //    if (!queryListRequest.Success)
        //    {
        //        _logger.Error(TraceType.ROUTE.ToString(), CallResultStatus.ERROR.ToString(), traceService, requestService, "BizContent解析失败", queryListRequest.FirstException, _request.BizContent);
        //        return new ObjectResult(null, ErrorCode.BIZ_CONTENT_DESERIALIZE_FAILED);
        //    }
        //    queryListRequest.Value.AppId = _request.AppId;

        //    traceMethod = $"{_service.GetType().FullName}.{nameof(_service.GetBoundCards)}(...)";
        //    _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), traceService, traceMethod, LogPhase.BEGIN, "开始查询个人绑卡列表", queryListRequest.Value);

        //    var queryListResult = _service.GetBoundCards(queryListRequest.Value);

        //    _logger.Trace(TraceType.ROUTE.ToString(), (queryListResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), traceService, traceMethod, LogPhase.END, "结束查询个人绑卡列表", queryListResult.Value);

        //    return queryListResult.Success ? new ObjectResult(queryListResult.Value) : new ObjectResult(null, queryListResult.ErrorCode, queryListResult.FirstException);
        //}

        //private ObjectResult CancelBindCard_1_0(String traceService, String requestService, ref String traceMethod)
        //{
        //    var cancelRequest = JsonUtil.DeserializeObject<PersonalCancelBoundCardRequest>(_request.BizContent);
        //    if (!cancelRequest.Success)
        //    {
        //        _logger.Error(TraceType.ROUTE.ToString(), CallResultStatus.ERROR.ToString(), traceService, requestService, "BizContent解析失败", cancelRequest.FirstException, _request.BizContent);
        //        return new ObjectResult(null, ErrorCode.BIZ_CONTENT_DESERIALIZE_FAILED);
        //    }
        //    cancelRequest.Value.AppId = _request.AppId;

        //    traceMethod = $"{_service.GetType().FullName}.{nameof(_service.CancelBoundCard)}(...)";
        //    _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), traceService, traceMethod, LogPhase.BEGIN, "开始取消个人绑卡", cancelRequest.Value);

        //    var cancelResult = _service.CancelBoundCard(cancelRequest.Value);

        //    _logger.Trace(TraceType.ROUTE.ToString(), (cancelResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), traceService, traceMethod, LogPhase.END, "结束取消个人绑卡", cancelResult.Value);

        //    return cancelResult.Success ? new ObjectResult(cancelResult.Value) : new ObjectResult(null, cancelResult.ErrorCode, cancelResult.FirstException);
        //}

        //private ObjectResult ReBindCard_1_0(String traceService, String requestService, ref String traceMethod)
        //{
        //    var rebindRequest = JsonUtil.DeserializeObject<PersonalWithdrawRebindCardRequest>(_request.BizContent);
        //    if (!rebindRequest.Success)
        //    {
        //        _logger.Error(TraceType.ROUTE.ToString(), CallResultStatus.ERROR.ToString(), traceService, requestService, "BizContent解析失败", rebindRequest.FirstException, _request.BizContent);
        //        return new ObjectResult(null, ErrorCode.BIZ_CONTENT_DESERIALIZE_FAILED);
        //    }
        //    rebindRequest.Value.AppId = _request.AppId;

        //    traceMethod = $"{_service.GetType().FullName}.{nameof(_service.WithdrawRebindCard)}(...)";
        //    _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), traceService, traceMethod, LogPhase.BEGIN, "开始个人重新绑卡", rebindRequest.Value);

        //    var rebindResult = _service.WithdrawRebindCard(rebindRequest.Value);

        //    _logger.Trace(TraceType.ROUTE.ToString(), (rebindResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), traceService, traceMethod, LogPhase.END, "结束个人重新绑卡", rebindResult.Value);

        //    return rebindResult.Success ? new ObjectResult(rebindResult.Value) : new ObjectResult(null, rebindResult.ErrorCode, rebindResult.FirstException);
        //}

        //private ObjectResult BindCard_1_0(String traceService, String requestService, ref String traceMethod)
        //{
        //    var bindRequest = JsonUtil.DeserializeObject<PersonalWithdrawBindCardRequest>(_request.BizContent);
        //    if (!bindRequest.Success)
        //    {
        //        _logger.Error(TraceType.ROUTE.ToString(), CallResultStatus.ERROR.ToString(), traceService, requestService, "BizContent解析失败", bindRequest.FirstException, _request.BizContent);
        //        return new ObjectResult(null, ErrorCode.BIZ_CONTENT_DESERIALIZE_FAILED);
        //    }
        //    bindRequest.Value.AppId = _request.AppId;

        //    traceMethod = $"{_service.GetType().FullName}.{nameof(_service.WithdrawBindCard)}(...)";
        //    _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), traceService, traceMethod, LogPhase.BEGIN, "开始个人提现绑卡", bindRequest.Value);

        //    var bindResult = _service.WithdrawBindCard(bindRequest.Value);

        //    _logger.Trace(TraceType.ROUTE.ToString(), (bindResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), traceService, traceMethod, LogPhase.END, "结束个人提现绑卡", bindResult.Value);

        //    return bindResult.Success ? new ObjectResult(bindResult.Value) : new ObjectResult(null, bindResult.ErrorCode, bindResult.FirstException);
        //}

        //private ObjectResult QueryInfo_1_0(String traceService, String requestService, ref String traceMethod)
        //{
        //    var queryInfoRequest = JsonUtil.DeserializeObject<PersonalInfoQueryRequest>(_request.BizContent);
        //    if (!queryInfoRequest.Success)
        //    {
        //        _logger.Error(TraceType.ROUTE.ToString(), CallResultStatus.ERROR.ToString(), traceService, requestService, "BizContent解析失败", queryInfoRequest.FirstException, _request.BizContent);
        //        return new ObjectResult(null, ErrorCode.BIZ_CONTENT_DESERIALIZE_FAILED);
        //    }
        //    queryInfoRequest.Value.AppId = _request.AppId;

        //    traceMethod = $"{_service.GetType().FullName}.{nameof(_service.GetAccountInfo)}(...)";
        //    _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), traceService, traceMethod, LogPhase.BEGIN, "开始查询个人账户信息", queryInfoRequest.Value);

        //    var queryInfoResult = _service.GetAccountInfo(queryInfoRequest.Value);

        //    _logger.Trace(TraceType.ROUTE.ToString(), (queryInfoResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), traceService, traceMethod, LogPhase.END, "结束查询个人账户信息", queryInfoResult.Value);

        //    return queryInfoResult.Success ? new ObjectResult(queryInfoResult.Value) : new ObjectResult(null, queryInfoResult.ErrorCode, queryInfoResult.FirstException);
        //}

        //private ObjectResult UpdateInfo_1_0(String traceService, String requestService, ref String traceMethod)
        //{
        //    var updateRequest = JsonUtil.DeserializeObject<PersonalInfoUpdateRequest>(_request.BizContent);
        //    if (!updateRequest.Success)
        //    {
        //        _logger.Error(TraceType.ROUTE.ToString(), CallResultStatus.ERROR.ToString(), traceService, requestService, "BizContent解析失败", updateRequest.FirstException, _request.BizContent);
        //        return new ObjectResult(null, ErrorCode.BIZ_CONTENT_DESERIALIZE_FAILED);
        //    }
        //    updateRequest.Value.AppId = _request.AppId;

        //    traceMethod = $"{_service.GetType().FullName}.{nameof(_service.UpdateAccountInfo)}(...)";
        //    _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), traceService, traceMethod, LogPhase.BEGIN, "开始更新个人账户信息", updateRequest.Value);

        //    var updateResult = _service.UpdateAccountInfo(updateRequest.Value);

        //    _logger.Trace(TraceType.ROUTE.ToString(), (updateResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), traceService, traceMethod, LogPhase.END, "结束更新个人账户信息", updateResult.Value);

        //    return updateResult.Success ? new ObjectResult(updateResult.Value) : new ObjectResult(null, updateResult.ErrorCode, updateResult.FirstException);
        //}

        //private ObjectResult Register_1_0(String traceService, String requestService, ref String traceMethod)
        //{
        //    var regRequest = JsonUtil.DeserializeObject<PersonalRegisterRequest>(_request.BizContent);
        //    if (!regRequest.Success)
        //    {
        //        _logger.Error(TraceType.ROUTE.ToString(), CallResultStatus.ERROR.ToString(), traceService, requestService, "BizContent解析失败", regRequest.FirstException, _request.BizContent);
        //        return new ObjectResult(null, ErrorCode.BIZ_CONTENT_DESERIALIZE_FAILED);
        //    }
        //    regRequest.Value.AppId = _request.AppId;

        //    traceMethod = $"{_service.GetType().FullName}.{nameof(_service.Register)}(...)";
        //    _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), traceService, traceMethod, LogPhase.BEGIN, "开始个人开户", regRequest.Value);

        //    var regResult = _service.Register(regRequest.Value);

        //    _logger.Trace(TraceType.ROUTE.ToString(), (regResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), traceService, traceMethod, LogPhase.END, "结束个人开户", regResult.Value);

        //    return regResult.Success ? new ObjectResult(regResult.Value) : new ObjectResult(null, regResult.ErrorCode, regResult.FirstException);
        //}
        #endregion

        #region #v1.1
        private ObjectResult Register_1_1(String traceService, String requestService, ref String traceMethod)
        {
            var regRequest = JsonUtil.DeserializeObject<PersonalRegisterRequestV1>(_request.BizContent);
            if (!regRequest.Success)
            {
                _logger.Error(TraceType.ROUTE.ToString(), CallResultStatus.ERROR.ToString(), traceService, requestService, "BizContent解析失败", regRequest.FirstException, _request.BizContent);
                return new ObjectResult(null, ErrorCode.BIZ_CONTENT_DESERIALIZE_FAILED);
            }
            regRequest.Value.AppId = _request.AppId;

            traceMethod = $"{_serviceV1.GetType().FullName}.{nameof(_serviceV1.Register)}(...)";
            _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), traceService, traceMethod, LogPhase.BEGIN, "开始个人开户", regRequest.Value);

            var regResult = _serviceV1.Register(regRequest.Value);

            _logger.Trace(TraceType.ROUTE.ToString(), (regResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), traceService, traceMethod, LogPhase.END, "结束个人开户", regResult.Value);

            return regResult.Success ? new ObjectResult(regResult.Value) : new ObjectResult(null, regResult.ErrorCode, regResult.FirstException);
        }

        private ObjectResult QueryRegisterInfo_1_1(String traceService, String requestService, ref String traceMethod)
        {
            var queryRequest = JsonUtil.DeserializeObject<PersonalRegisterInfoQueryRequestV1>(_request.BizContent);
            if (!queryRequest.Success)
            {
                _logger.Error(TraceType.ROUTE.ToString(), CallResultStatus.ERROR.ToString(), traceService, requestService, "BizContent解析失败", queryRequest.FirstException, _request.BizContent);
                return new ObjectResult(null, ErrorCode.BIZ_CONTENT_DESERIALIZE_FAILED);
            }
            queryRequest.Value.AppId = _request.AppId;

            traceMethod = $"{_serviceV1.GetType().FullName}.{nameof(_serviceV1.QueryPersonalInfo)}(...)";
            _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), traceService, traceMethod, LogPhase.BEGIN, "开始查询个人开户信息", queryRequest.Value);

            var queryResult = _serviceV1.QueryPersonalInfo(queryRequest.Value);

            _logger.Trace(TraceType.ROUTE.ToString(), (queryResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), traceService, traceMethod, LogPhase.END, "结束查询个人开户信息", queryResult.Value);

            return queryResult.Success ? new ObjectResult(queryResult.Value) : new ObjectResult(null, queryResult.ErrorCode, queryResult.FirstException);
        }

        private ObjectResult QueryAccounts_1_1(String traceService, String requestService, ref String traceMethod)
        {
            var queryInfoRequest = JsonUtil.DeserializeObject<PersonalRegesiterAccountsQueryRequestV1>(_request.BizContent);
            if (!queryInfoRequest.Success)
            {
                _logger.Error(TraceType.ROUTE.ToString(), CallResultStatus.ERROR.ToString(), traceService, requestService, "BizContent解析失败", queryInfoRequest.FirstException, _request.BizContent);
                return new ObjectResult(null, ErrorCode.BIZ_CONTENT_DESERIALIZE_FAILED);
            }
            queryInfoRequest.Value.AppId = _request.AppId;

            traceMethod = $"{_service.GetType().FullName}.{nameof(_service.GetAccountInfo)}(...)";
            _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), traceService, traceMethod, LogPhase.BEGIN, "开始查询个人账户信息", queryInfoRequest.Value);

            var queryInfoResult = _serviceV1.QueryPersonalRegesiterAccounts(queryInfoRequest.Value);

            _logger.Trace(TraceType.ROUTE.ToString(), (queryInfoResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), traceService, traceMethod, LogPhase.END, "结束查询个人账户信息", queryInfoResult.Value);

            return queryInfoResult.Success ? new ObjectResult(queryInfoResult.Value) : new ObjectResult(null, queryInfoResult.ErrorCode, queryInfoResult.FirstException);
        }

        private ObjectResult SignContract_1_1(String traceService, String requestService, ref String traceMethod)
        {
            var signRequest = JsonUtil.DeserializeObject<PersonalRegisterContractSignRequestV1>(_request.BizContent);
            if (!signRequest.Success)
            {
                _logger.Error(TraceType.ROUTE.ToString(), CallResultStatus.ERROR.ToString(), traceService, requestService, "BizContent解析失败", signRequest.FirstException, _request.BizContent);
                return new ObjectResult(null, ErrorCode.BIZ_CONTENT_DESERIALIZE_FAILED);
            }
            signRequest.Value.AppId = _request.AppId;

            traceMethod = $"{_serviceV1.GetType().FullName}.{nameof(_serviceV1.SignContract)}(...)";
            _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), traceService, traceMethod, LogPhase.BEGIN, "开始合同签约", signRequest.Value);

            var signResult = _serviceV1.SignContract(signRequest.Value);

            _logger.Trace(TraceType.ROUTE.ToString(), (signResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), traceService, traceMethod, LogPhase.END, "结束合同签约", signResult.Value);

            return signResult.Success ? new ObjectResult(signResult.Value) : new ObjectResult(null, signResult.ErrorCode, signResult.FirstException);
        }

        private ObjectResult ContractInfo_1_1(String traceService, String requestService, ref String traceMethod)
        {
            var regRequest = JsonUtil.DeserializeObject<PersonalRegisterContractInfoQueryRequestV1>(_request.BizContent);
            if (!regRequest.Success)
            {
                _logger.Error(TraceType.ROUTE.ToString(), CallResultStatus.ERROR.ToString(), traceService, requestService, "BizContent解析失败", regRequest.FirstException, _request.BizContent);
                return new ObjectResult(null, ErrorCode.BIZ_CONTENT_DESERIALIZE_FAILED);
            }
            regRequest.Value.AppId = _request.AppId;

            traceMethod = $"{_serviceV1.GetType().FullName}.{nameof(_serviceV1.QueryContract)}(...)";
            _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), traceService, traceMethod, LogPhase.BEGIN, "开始查询合同", regRequest.Value);

            var regResult = _serviceV1.QueryContract(regRequest.Value);

            _logger.Trace(TraceType.ROUTE.ToString(), (regResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), traceService, traceMethod, LogPhase.END, "结束查询合同", regResult.Value);

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

            traceMethod = $"{_serviceV1.GetType().FullName}.{nameof(_serviceV1.QueryBankCardAccept)}(...)";
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

            traceMethod = $"{_service.GetType().FullName}.{nameof(_serviceV1.ApplyBindCard)}(...)";
            _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), traceService, traceMethod, LogPhase.BEGIN, "开始申请提现绑卡", applyRequest.Value);

            var applyResult = _serviceV1.ApplyBindCard(applyRequest.Value);

            _logger.Trace(TraceType.ROUTE.ToString(), (applyResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), traceService, traceMethod, LogPhase.END, "结束申请提现绑卡", applyResult.Value);

            return applyResult.Success ? new ObjectResult(applyResult.Value) : new ObjectResult(null, applyResult.ErrorCode, applyResult.FirstException);
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

            traceMethod = $"{_serviceV1.GetType().FullName}.{nameof(_serviceV1.WithdrawBindCard)}(...)";
            _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), traceService, traceMethod, LogPhase.BEGIN, "开始个人提现绑卡", bindRequest.Value);

            var bindResult = _serviceV1.WithdrawBindCard(bindRequest.Value);

            _logger.Trace(TraceType.ROUTE.ToString(), (bindResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), traceService, traceMethod, LogPhase.END, "结束个人提现绑卡", bindResult.Value);

            return bindResult.Success ? new ObjectResult(bindResult.Value) : new ObjectResult(null, bindResult.ErrorCode, bindResult.FirstException);
        }

        private ObjectResult Withdraw_1_1(String traceService, String requestService, ref String traceMethod)
        {
            var withdrawRequest = JsonUtil.DeserializeObject<PersonalWithdrawRequestV1>(_request.BizContent);
            if (!withdrawRequest.Success)
            {
                _logger.Error(TraceType.ROUTE.ToString(), CallResultStatus.ERROR.ToString(), traceService, requestService, "BizContent解析失败", withdrawRequest.FirstException, _request.BizContent);
                return new ObjectResult(null, ErrorCode.BIZ_CONTENT_DESERIALIZE_FAILED);
            }
            withdrawRequest.Value.AppId = _request.AppId;

            traceMethod = $"{_serviceV1.GetType().FullName}.{nameof(_serviceV1.ApplyWithdraw)}(...)";
            _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), traceService, traceMethod, LogPhase.BEGIN, "开始个人提现", withdrawRequest.Value);

            var withdrawResult = _serviceV1.ApplyWithdraw(withdrawRequest.Value);

            _logger.Trace(TraceType.ROUTE.ToString(), (withdrawResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), traceService, traceMethod, LogPhase.END, "结束个人提现", withdrawResult.Value);

            return withdrawResult.Success ? new ObjectResult(withdrawResult.Value) : new ObjectResult(null, withdrawResult.ErrorCode, withdrawResult.FirstException);
        }

        private ObjectResult QueryWithdrawOrder_1_1(String traceService, String requestService, ref String traceMethod)
        {
            var withdrawOrderQueryRequest = JsonUtil.DeserializeObject<WithdrawOrderQueryRequestV1>(_request.BizContent);
            if (!withdrawOrderQueryRequest.Success)
            {
                _logger.Error(TraceType.ROUTE.ToString(), CallResultStatus.ERROR.ToString(), traceService, requestService, "BizContent解析失败", withdrawOrderQueryRequest.FirstException, _request.BizContent);
                return new ObjectResult(null, ErrorCode.BIZ_CONTENT_DESERIALIZE_FAILED);
            }
            withdrawOrderQueryRequest.Value.AppId = _request.AppId;

            traceMethod = $"{_serviceV1.GetType().FullName}.{nameof(_serviceV1.QueryWithdrawOrder)}(...)";
            _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), traceService, traceMethod, LogPhase.BEGIN, "开始查询提现订单", withdrawOrderQueryRequest.Value);

            var queryResult = _serviceV1.QueryWithdrawOrder(withdrawOrderQueryRequest.Value);

            _logger.Trace(TraceType.ROUTE.ToString(), (queryResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), traceService, traceMethod, LogPhase.END, "结束查询提现订单", queryResult.Value);

            return queryResult.Success ? new ObjectResult(queryResult.Value) : new ObjectResult(null, queryResult.ErrorCode, queryResult.FirstException);
        }

        private ObjectResult QueryWithdrawOrderList_1_1(String traceService, String requestService, ref String traceMethod)
        {
            var withdrawOrderQueryRequest = JsonUtil.DeserializeObject<WithdrawOrderListQueryRequestV1>(_request.BizContent);
            if (!withdrawOrderQueryRequest.Success)
            {
                _logger.Error(TraceType.ROUTE.ToString(), CallResultStatus.ERROR.ToString(), traceService, requestService, "BizContent解析失败", withdrawOrderQueryRequest.FirstException, _request.BizContent);
                return new ObjectResult(null, ErrorCode.BIZ_CONTENT_DESERIALIZE_FAILED);
            }
            withdrawOrderQueryRequest.Value.AppId = _request.AppId;

            traceMethod = $"{_serviceV1.GetType().FullName}.{nameof(_serviceV1.QueryWithdrawOrderList)}(...)";
            _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), traceService, traceMethod, LogPhase.BEGIN, "开始查询提现订单", withdrawOrderQueryRequest.Value);

            var queryResult = _serviceV1.QueryWithdrawOrderList(withdrawOrderQueryRequest.Value);

            _logger.Trace(TraceType.ROUTE.ToString(), (queryResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), traceService, traceMethod, LogPhase.END, "结束查询提现订单", queryResult.Value);

            return queryResult.Success ? new ObjectResult(queryResult.Value) : new ObjectResult(null, queryResult.ErrorCode, queryResult.FirstException);
        }

        private ObjectResult AccountBalance_1_1(String traceService, String requestService, ref String traceMethod)
        {
            var queryRequest = JsonUtil.DeserializeObject<PersonalAccountBalanceQueryRequestV1>(_request.BizContent);
            if (!queryRequest.Success)
            {
                _logger.Error(TraceType.ROUTE.ToString(), CallResultStatus.ERROR.ToString(), traceService, requestService, "BizContent解析失败", queryRequest.FirstException, _request.BizContent);
                return new ObjectResult(null, ErrorCode.BIZ_CONTENT_DESERIALIZE_FAILED);
            }
            queryRequest.Value.AppId = _request.AppId;

            traceMethod = $"{_serviceV1.GetType().FullName}.{nameof(_serviceV1.QueryAccountBalance)}(...)";
            _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), traceService, traceMethod, LogPhase.BEGIN, "开始查询账户余额", queryRequest.Value);

            var queryResult = _serviceV1.QueryAccountBalance(queryRequest.Value);

            _logger.Trace(TraceType.ROUTE.ToString(), (queryResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), traceService, traceMethod, LogPhase.END, "结束查询账户余额", queryResult.Value);

            return queryResult.Success ? new ObjectResult(queryResult.Value) : new ObjectResult(null, queryResult.ErrorCode, queryResult.FirstException);
        }

        private ObjectResult PayResultPull_1_1(String traceService, String requestService, ref String traceMethod)
        {
            var pullRequest = JsonUtil.DeserializeObject<PersonalWithdrawResultPullRequestV1>(_request.BizContent);
            if (!pullRequest.Success)
            {
                _logger.Error(TraceType.ROUTE.ToString(), CallResultStatus.ERROR.ToString(), traceService, requestService, "BizContent解析失败", pullRequest.FirstException, _request.BizContent);
                return new ObjectResult(0, ErrorCode.BIZ_CONTENT_DESERIALIZE_FAILED);
            }
            pullRequest.Value.AppId = _request.AppId;

            traceMethod = $"{_serviceV1.GetType().FullName}.{nameof(_serviceV1.PullWithdrawResult)}(...)";

            _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), traceService, traceMethod, LogPhase.BEGIN, $"开始拉取提现状态", pullRequest.Value);

            var pullResult = _serviceV1.PullWithdrawResult(pullRequest.Value);

            _logger.Trace(TraceType.ROUTE.ToString(), (pullResult.Success ? CallResultStatus.OK : CallResultStatus.ERROR).ToString(), traceService, traceMethod, LogPhase.END, $"结束拉取提现状态", pullResult.Value);

            return pullResult.Success ? new ObjectResult(new PersonalWithdrawResultPullResponseV1()
            {
                SuccessCount = pullResult.Value.SuccessCount
            }) : new ObjectResult(null, pullResult.ErrorCode, pullResult.FirstException);
        }
        #endregion
    }
}
