using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPI.Common.Models;
using CPI.IData.BaseRepositories;
using CPI.IService.BaseServices;

namespace CPI.Services.BaseServices
{
    public class BankCardBinService : IBankCardBinService
    {
        private IBankCardBinRepository _bankCardBinRepository = null;

        public BankCardBin GetBankCardBin(String bankCardNo)
        {
            if (String.IsNullOrWhiteSpace(bankCardNo)) {
                return null;
            }

            var bin = (from t0 in _bankCardBinRepository.QueryProvider
                      where bankCardNo.StartsWith(t0.CardBin)
                      select t0).FirstOrDefault();

            return bin;
        }
    }
}
