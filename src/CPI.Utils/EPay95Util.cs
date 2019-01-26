using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using CPI.Common;
using CPI.Common.Domain.FundOut.EPay95;
using CPI.Config;
using Lotus.Core;
using Lotus.Logging;
using Lotus.Security;

namespace CPI.Utils
{
    public static class EPay95Util
    {
        private static readonly ILogger _logger = LogManager.GetLogger();

        public static String MakeSign(Dictionary<String, String> dic)
        {
            if (dic == null || dic.Count == 0)
            {
                return null;
            }

            String signContent = String.Empty;

            foreach (var kv in dic)
            {
                signContent += kv.Value;
            }

            var signResult = CryptoHelper.MakeSign(signContent, KeyConfig.EPay95_FundOut_Hehua_PrivateKey, PrivateKeyFormat.PKCS8, HashAlgorithmName.SHA1);
            if (!signResult.Success)
            {
                return null;
            }

            return signResult.Value;
        }

        public static Boolean VerifySign(PayNotifyResult value)
        {
            if (value == null) { return false; }

            String signContent = String.Empty;
            signContent += value.LoanJsonList;
            signContent += value.PlatformMoneymoremore;
            signContent += value.BatchNo;
            signContent += value.Remark;
            signContent += value.ResultCode;

            _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.OK.ToString(), $"{nameof(EPay95Util)}.VerifySign(...)", "LoanJsonList", LogPhase.ACTION, "LoanJsonList", value.LoanJsonList);
            _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.OK.ToString(), $"{nameof(EPay95Util)}.VerifySign(...)", "PlatformMoneymoremore", LogPhase.ACTION, "PlatformMoneymoremore", value.PlatformMoneymoremore);
            _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.OK.ToString(), $"{nameof(EPay95Util)}.VerifySign(...)", "BatchNo", LogPhase.ACTION, "BatchNo", value.BatchNo);
            _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.OK.ToString(), $"{nameof(EPay95Util)}.VerifySign(...)", "Remark", LogPhase.ACTION, "Remark", value.Remark);
            _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.OK.ToString(), $"{nameof(EPay95Util)}.VerifySign(...)", "ResultCode", LogPhase.ACTION, "ResultCode", value.ResultCode);
            _logger.Trace(TraceType.BLL.ToString(), CallResultStatus.OK.ToString(), $"{nameof(EPay95Util)}.VerifySign(...)", "signContent", LogPhase.ACTION, "SignContent", signContent);

            var verifyResult = CryptoHelper.VerifySign(value.SignInfo, signContent, KeyConfig.EPay95_FundOut_PublicKey, HashAlgorithmName.SHA1);
            return verifyResult.Success && verifyResult.Value;
        }
    }
}
