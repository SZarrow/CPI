using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using CPI.Common;
using CPI.Common.Domain.SettleDomain.Bill99;
using CPI.Common.Exceptions;
using CPI.IService.SettleServices;
using CPI.Utils;
using ATBase.Core;

namespace CPI.Services.SettleServices
{
    public class AccountService : IAccountService
    {
        public XResult<AccountBalanceQueryResponse> GetBalance(AccountBalanceQueryRequest request)
        {
            var queryResult = Bill99UtilYZT.Execute<RawAccountBalanceQueryRequest, RawAccountBalanceQueryResponse>("/account/balance/query", new RawAccountBalanceQueryRequest()
            {
                accountBalanceType = request.AccountBalanceTypes,
                uId = request.PayeeId
            });

            if (!queryResult.Success)
            {
                return new XResult<AccountBalanceQueryResponse>(null, ErrorCode.DEPENDENT_API_CALL_FAILED, queryResult.FirstException);
            }

            if (queryResult.Value == null)
            {
                return new XResult<AccountBalanceQueryResponse>(null, ErrorCode.REMOTE_RETURN_NOTHING);
            }

            if (queryResult.Value.ResponseCode != "0000")
            {
                return new XResult<AccountBalanceQueryResponse>(null, ErrorCode.DEPENDENT_API_CALL_FAILED, new RemoteException(queryResult.Value.ResponseMessage));
            }

            return new XResult<AccountBalanceQueryResponse>(new AccountBalanceQueryResponse()
            {
                AccountBalances = from t0 in queryResult.Value.accountBalanceList
                                  select new AccountBalanceInfo()
                                  {
                                      AccountBalanceType = t0.accountBalanceType,
                                      AccountName = t0.accountName,
                                      AvailableBalance = t0.availableBalance,
                                      Balance = t0.balance
                                  }
            });
        }
    }
}
