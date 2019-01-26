using System;
using System.Collections.Generic;
using System.Text;
using CPI.Common.Models;

namespace CPI.IService.BaseServices
{
    /// <summary>
    /// 银行卡BIN服务接口
    /// </summary>
    public interface IBankCardBinService
    {
        /// <summary>
        /// 获取指定银行卡号的BIN信息
        /// </summary>
        /// <param name="bankCardNo">银行卡号</param>
        BankCardBin GetBankCardBin(String bankCardNo);
    }
}
