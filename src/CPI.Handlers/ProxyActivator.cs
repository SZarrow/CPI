using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPI.Common;
using CPI.Common.Domain;
using CPI.Common.Domain.AgreePay;
using CPI.Config;
using CPI.Handlers.AgreePay;
using CPI.Handlers.EntrustPay;
using CPI.Handlers.FundOut;
using CPI.Handlers.Settle;
using CPI.IService.BaseServices;
using CPI.Utils;
using Lotus.Core;
using Lotus.Logging;

namespace CPI.Handlers
{
    public static class ProxyActivator
    {
        private static readonly ILogger _logger = LogManager.GetLogger();

        public static IInvocation GetInvocation(GatewayCommonRequest request)
        {
            if (request == null || !request.IsValid)
            {
                return null;
            }

            String service = $"{typeof(ProxyActivator).FullName}.GetInvocation()";
            String tag = $"{request.Method}.{request.Version}";

            if (request.Method.IndexOf("cpi.fundout.single.99bill.") == 0)
            {
                _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), service, tag, LogPhase.ACTION, "创建 Bill99SinglePayInvocation");
                return new Bill99SinglePayInvocation(request);
            }

            if (request.Method.IndexOf("cpi.fundout.single.95epay.") == 0)
            {
                _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), service, tag, LogPhase.ACTION, "创建 EPay95SinglePayInvocation");
                return new EPay95SinglePayInvocation(request);
            }

            if (request.Method.IndexOf("cpi.fundout.single.yeepay.") == 0)
            {
                _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), service, tag, LogPhase.ACTION, "创建 YeePaySinglePayInvocation");
                return new YeePaySinglePayInvocation(request);
            }

            if (request.Method.IndexOf("cpi.settle.personal.") == 0)
            {
                _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), service, tag, LogPhase.ACTION, "创建 Bill99PersonalInvocation");
                return new Bill99PersonalInvocation(request);
            }

            if (request.Method.IndexOf("cpi.settle.allot.") == 0)
            {
                _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), service, tag, LogPhase.ACTION, "创建 Bill99AllotAmountInvocation");
                return new Bill99AllotAmountInvocation(request);
            }

            if (request.Method.IndexOf("cpi.settle.allotamount.withdraw.") == 0
                || request.Method.IndexOf("cpi.settle.withdraw.") == 0)
            {
                _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), service, tag, LogPhase.ACTION, "创建 Bill99WithdrawInvocation");
                return new Bill99WithdrawInvocation(request);
            }

            if (request.Method.IndexOf("cpi.settle.account.") == 0)
            {
                _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), service, tag, LogPhase.ACTION, "创建 Bill99AccountInvocation");
                return new Bill99AccountInvocation(request);
            }

            if (request.Method.IndexOf("cpi.agreepay.apply.99bill") == 0
                || request.Method.IndexOf("cpi.agreepay.bindcard.99bill") == 0
                || request.Method.IndexOf("cpi.agreepay.pay.99bill") == 0)
            {
                _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), service, tag, LogPhase.ACTION, "创建 Bill99AgreePayInvocation");
                return new Bill99AgreePayInvocation(request);
            }

            if (request.Method.IndexOf("cpi.agreepay.apply.yeepay") == 0
                || request.Method.IndexOf("cpi.agreepay.bindcard.yeepay") == 0
                || request.Method.IndexOf("cpi.agreepay.pay.yeepay") == 0
                || request.Method.IndexOf("cpi.agreepay.refund.yeepay") == 0
                || request.Method.IndexOf("cpi.agreepay.payresult.pull.yeepay") == 0)
            {
                _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), service, tag, LogPhase.ACTION, "创建 Bill99AgreePayInvocation");
                return new YeePayAgreePayInvocation(request);
            }

            if (request.Method.IndexOf("cpi.entrust.pay.99bill") == 0)
            {
                _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), service, tag, LogPhase.ACTION, "创建 Bill99EntrustPayInvocation");
                return new Bill99EntrustPayInvocation(request);
            }

            if (request.Method.IndexOf("cpi.unified.querystatus") == 0)
            {
                _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), service, tag, LogPhase.ACTION, "创建 AgreePayInvocation");
                return new AgreePayInvocation(request);
            }

            if (request.Method.IndexOf("cpi.unified.payresult.pull") == 0)
            {
                _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), service, tag, LogPhase.ACTION, "创建 Bill99AgreePayInvocation");
                return new Bill99AgreePayInvocation(request);
            }

            if (request.Method.IndexOf("cpi.unified.pay") == 0)
            {
                // 根据AppId从数据库中查出默认通道编码，如果没有指定则默认使用快钱
                var appChannelRouteService = XDI.Resolve<IAppChannelRouteService>();
                if (appChannelRouteService == null)
                {
                    _logger.Error(TraceType.ROUTE.ToString(), CallResultStatus.ERROR.ToString(), service, tag, "未能初始化IAppChannelRouteService的实例", null, request);
                    return null;
                }

                var channelRouteInfo = appChannelRouteService.GetPayChannel(request.AppId);
                request.Method += $".{(channelRouteInfo != null ? channelRouteInfo.ChannelCode : GlobalConfig.DefaultPayChannelCode)}";

                // 确定通道之后取出所有通道的费率，
                // 然后根据当前支付金额算出每个通道的费用，
                // 然后得到费用最小的一个通道，
                // 然后对于这个费用最小的通道，通道费用大于阈值的用代扣，否则用协议支付。

                var paymentRequest = JsonUtil.DeserializeObject<CommonPayRequest>(request.BizContent);
                if (!paymentRequest.Success)
                {
                    _logger.Error(TraceType.ROUTE.ToString(), CallResultStatus.ERROR.ToString(), service, tag, "解析支付金额失败", paymentRequest.FirstException, request);
                    return null;
                }

                var payChannelService = XDI.Resolve<IPayChannelService>();
                var channels = payChannelService.GetAllChannels();
                if (channels != null && channels.Count() > 0)
                {
                    var cheapestChannel = (from ch in channels
                                           let cost = paymentRequest.Value.GetPayAmount() * ch.PayRate
                                           orderby cost
                                           select new
                                           {
                                               ch.ChannelCode,
                                               Cost = cost
                                           }).FirstOrDefault();

                    if (cheapestChannel.Cost > GlobalConfig.PayChannelFeeThreshold)
                    {
                        _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), service, tag, LogPhase.ACTION, "创建 Bill99EntrustPayInvocation");
                        return CreateEntrustPayInvocation(cheapestChannel.ChannelCode, request);
                    }

                    return CreateAgreePayInvocation(cheapestChannel.ChannelCode, request);
                }

                _logger.Trace(TraceType.ROUTE.ToString(), CallResultStatus.OK.ToString(), service, tag, LogPhase.ACTION, "创建 Bill99AgreePayInvocation");
                return CreateAgreePayInvocation(GlobalConfig.X99BILL_PAYCHANNEL_CODE, request);
            }

            return null;
        }

        private static IInvocation CreateAgreePayInvocation(String channelCode, GatewayCommonRequest request)
        {
            switch (channelCode)
            {
                case GlobalConfig.X99BILL_PAYCHANNEL_CODE:
                    return new Bill99AgreePayInvocation(request);
                case GlobalConfig.YEEPAY_PAYCHANNEL_CODE:
                    return new YeePayAgreePayInvocation(request);
            }

            return null;
        }

        private static IInvocation CreateEntrustPayInvocation(String channelCode, GatewayCommonRequest request)
        {
            switch (channelCode)
            {
                case GlobalConfig.X99BILL_PAYCHANNEL_CODE:
                    return new Bill99EntrustPayInvocation(request);
                case GlobalConfig.YEEPAY_PAYCHANNEL_CODE:
                    return null;
            }

            return null;
        }
    }
}
