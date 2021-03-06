﻿using System;
using System.Collections.Generic;
using System.Text;
using ATBase.Core.Collections;

namespace CPI.Common.Domain.SettleDomain.Bill99.v1_0
{
    public class WithdrawOrderListQueryResponseV1 : CommonResponse
    {
        public IEnumerable<WithdrawOrderListQueryItem> Orders { get; set; }
        public Int32 SuccessCount { get; set; }
        public Decimal SuccessAmount { get; set; }
        public Int32 PageIndex { get; set; }
        public Int32 PageCount { get; set; }
    }

    public class WithdrawOrderListQueryItem
    {
        public String Id { get; set; }
        public String OutTradeNo { get; set; }
        public String PayeeId { get; set; }
        public String IDCardNo { get; set; }
        public String RealName { get; set; }
        public String Mobile { get; set; }
        public String BankCardNo { get; set; }
        public Decimal Amount { get; set; }
        public String Status { get; set; }
        public DateTime ApplyTime { get; set; }
        public DateTime? CompleteTime { get; set; }
        public String Remark { get; set; }
    }
}
