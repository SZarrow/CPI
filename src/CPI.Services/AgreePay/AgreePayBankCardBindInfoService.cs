using System;
using System.Collections.Generic;
using System.Linq;
using CPI.Common.Domain.AgreePay;
using CPI.Common.Exceptions;
using CPI.IData.BaseRepositories;
using CPI.IService.AgreePay;
using Lotus.Core;

namespace CPI.Services.AgreePay
{
    public class AgreePayBankCardBindInfoService : IAgreePayBankCardBindInfoService
    {
        private readonly IAgreePayBankCardBindInfoRepository _agreePayBankCardBindInfoRepository = null;
        private readonly IAgreePayBankCardInfoRepository _agreePayBankCardInfoRepository = null;

        public XResult<IEnumerable<AgreePayBankCardBindDetail>> GetBankCardBindDetails(String payerId, String bankCardNo, String payChannelCode)
        {
            if (String.IsNullOrWhiteSpace(payerId))
            {
                return new XResult<IEnumerable<AgreePayBankCardBindDetail>>(null, new ArgumentNullException(nameof(payerId)));
            }

            if (String.IsNullOrWhiteSpace(bankCardNo))
            {
                return new XResult<IEnumerable<AgreePayBankCardBindDetail>>(null, new ArgumentNullException(nameof(bankCardNo)));
            }

            try
            {
                var boundDetails = (from t0 in _agreePayBankCardInfoRepository.QueryProvider
                                    join t1 in _agreePayBankCardBindInfoRepository.QueryProvider on t0.Id equals t1.BankCardId
                                    where t1.BankCardNo == bankCardNo && t1.PayerId == payerId && t1.PayChannelCode == payChannelCode
                                    select new AgreePayBankCardBindDetail()
                                    {
                                        PayerId = t1.PayerId,
                                        Mobile = t0.Mobile,
                                        IDCardNo = t0.IDCardNo,
                                        RealName = t0.RealName,
                                        OutTradeNo = t1.OutTradeNo,
                                        BankCardNo = t1.BankCardNo,
                                        PayChannelCode = t1.PayChannelCode,
                                        PayToken = t1.PayToken,
                                        BindStatus = t1.BindStatus,
                                        ApplyTime = t1.ApplyTime
                                    }).ToList();


                if (boundDetails == null || boundDetails.Count == 0)
                {
                    return new XResult<IEnumerable<AgreePayBankCardBindDetail>>(null, new RequestException("该用户尚未成功绑卡"));
                }

                return new XResult<IEnumerable<AgreePayBankCardBindDetail>>(boundDetails);
            }
            catch (Exception ex)
            {
                return new XResult<IEnumerable<AgreePayBankCardBindDetail>>(null, ex);
            }
        }
    }
}
