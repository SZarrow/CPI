using System;
using System.Collections.Generic;
using System.Text;
using CPI.Common;
using CPI.Common.Domain.AgreePay;
using CPI.Common.Domain.AgreePay.YeePay;
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

        //private readonly IAgreePayBankCardBindInfoRepository _bankCardBindInfoRepository = null;
        //private readonly IAgreePayBankCardInfoRepository _bankCardInfoRepository = null;
        //private readonly IPayOrderRepository _payOrderRepository = null;
        //private readonly IAllotAmountOrderRepository _allotAmountOrderRepository = null;

        public XResult<CPIAgreePayApplyResponse> Apply(CPIAgreePayApplyRequest request)
        {
            if (request == null)
            {
                return new XResult<CPIAgreePayApplyResponse>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentNullException(nameof(request)));
            }

            if (!request.IsValid)
            {
                return new XResult<CPIAgreePayApplyResponse>(null, ErrorCode.INVALID_ARGUMENT, new ArgumentException(request.ErrorMessage));
            }

            String service = $"{this.GetType().FullName}.Apply(...)";

            var requestHash = $"apply:{request.PayerId}.{request.BankCardNo}".GetHashCode();

            if (_lockProvider.Exists(requestHash))
            {
                return new XResult<CPIAgreePayApplyResponse>(null, ErrorCode.SUBMIT_REPEAT);
            }

            try
            {
                if (!_lockProvider.Lock(requestHash))
                {
                    return new XResult<CPIAgreePayApplyResponse>(null, ErrorCode.SUBMIT_REPEAT);
                }

                //// 如果未保存绑卡信息则添加到数据库
                //var existsBankCard = _bankCardInfoRepository.Exists(x => x.IDCardNo == request.IDCardNo && x.BankCardNo == request.BankCardNo);
                //if (!existsBankCard)
                //{
                //    // 先将绑卡的银行卡数据入库
                //    var bankCardInfo = new AgreePayBankCardInfo()
                //    {
                //        Id = IDGenerator.GenerateID(),
                //        AppId = request.AppId,
                //        RealName = request.RealName,
                //        IDCardNo = request.IDCardNo,
                //        BankCardNo = request.BankCardNo,
                //        Mobile = request.Mobile,
                //        BankCode = request.BankCode,
                //        UpdateTime = DateTime.Now
                //    };

                //    _bankCardInfoRepository.Add(bankCardInfo);
                //    var saveResult = _bankCardInfoRepository.SaveChanges();
                //    if (!saveResult.Success)
                //    {
                //        _logger.Error(TraceType.BLL.ToString(), CallResultStatus.ERROR.ToString(), service, $"{nameof(_bankCardInfoRepository)}.SaveChanges()", "快钱协议支付：保存绑卡信息失败", saveResult.FirstException, bankCardInfo);
                //        return new XResult<CPIAgreePayApplyResponse>(null, ErrorCode.DB_UPDATE_FAILED, saveResult.FirstException);
                //    }
                //}

                //调第三方接口
                var execResult = YeePayAgreePayUtil.Execute<RawYeePayApplyBindCardRequest, RawYeePayApplyBindCardResult>("/rest/v1.0/paperorder/unified/auth/request", new RawYeePayApplyBindCardRequest()
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
                    issms = true,
                    requesttime = "2019/3/4 15:23:50",// DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
                    authtype = "COMMON_FOUR"
                });

                return new XResult<CPIAgreePayApplyResponse>(null);
            }
            finally
            {
                _lockProvider.UnLock(requestHash);
            }
        }

        public XResult<CPIAgreePayBindCardResponse> BindCard(CPIAgreePayBindCardRequest request)
        {
            throw new NotImplementedException();
        }

        public XResult<CPIAgreePayPaymentResponse> Pay(CPIAgreePayPaymentRequest request)
        {
            throw new NotImplementedException();
        }

        public XResult<Int32> Pull(Int32 count)
        {
            throw new NotImplementedException();
        }

        public XResult<PagedList<CPIAgreePayQueryResult>> Query(CPIAgreePayQueryRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
