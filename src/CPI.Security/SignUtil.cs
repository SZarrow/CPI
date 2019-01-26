using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Lotus.Core;
using Lotus.Security;
using Newtonsoft.Json;

namespace CPI.Security
{
    public static class SignUtil
    {
        public static XResult<String> MakeSign(IDictionary<String, String> paraDic, String privateKey, PrivateKeyFormat privateKeyFormat, String signType = "RSA2")
        {
            if (paraDic == null || paraDic.Count == 0)
            {
                return new XResult<String>(null, new ArgumentNullException("paraDic is null"));
            }

            if (String.IsNullOrWhiteSpace(privateKey))
            {
                return new XResult<String>(null, new ArgumentNullException("privateKey is null"));
            }

            return CryptoHelper.MakeSign(GetSignContent(paraDic), privateKey, privateKeyFormat, GetHasAlgName(signType));
        }

        public static XResult<String> MakeSign(Object instance, String privateKey, PrivateKeyFormat privateKeyFormat, String signType = "RSA2")
        {
            var properties = from p in instance.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty)
                             where String.Compare(p.Name, "sign", true) != 0
                             select p;

            var dic = new Dictionary<String, String>(properties.Count());
            foreach (var p in properties)
            {
                var cusAttr = p.GetCustomAttribute<JsonPropertyAttribute>();
                var propertyValue = p.XGetValue(instance);
                if (propertyValue != null)
                {
                    dic[cusAttr != null ? cusAttr.PropertyName : p.Name.ToLower()] = propertyValue.ToString();
                }
            }

            return MakeSign(dic, privateKey, privateKeyFormat, signType);
        }

        public static XResult<String> MakeSign(String signContent, String privateKey, PrivateKeyFormat privateKeyFormat, String signType = "RSA2")
        {
            return CryptoHelper.MakeSign(signContent, privateKey, privateKeyFormat, GetHasAlgName(signType));
        }

        public static XResult<Byte[]> MakeSign(Byte[] signContent, String privateKey, PrivateKeyFormat privateKeyFormat, String signType = "RSA2")
        {
            return CryptoHelper.MakeSign(signContent, privateKey, privateKeyFormat, GetHasAlgName(signType));
        }

        public static XResult<Byte[]> MakeX509Sign(Byte[] signContent, String privateKeyFilePath, String privateKeyPassword, String signType = "SHA1")
        {
            if (!File.Exists(privateKeyFilePath))
            {
                return new XResult<Byte[]>(null, new FileNotFoundException(privateKeyFilePath));
            }

            X509Certificate2 cert = new X509Certificate2(privateKeyFilePath, privateKeyPassword);
            RSAPKCS1SignatureFormatter formatter = new RSAPKCS1SignatureFormatter(cert.PrivateKey);

            Byte[] rgbHash;
            switch (signType.ToUpperInvariant())
            {
                case "MD5":
                    formatter.SetHashAlgorithm("MD5");
                    var md5Result = CryptoHelper.GetMD5(signContent);
                    if (!md5Result.Success)
                    {
                        return new XResult<Byte[]>(null, md5Result.Exceptions.ToArray());
                    }
                    rgbHash = md5Result.Value;
                    break;
                default:
                    formatter.SetHashAlgorithm("SHA1");
                    var sha1Result = CryptoHelper.GetSHA1(signContent);
                    if (!sha1Result.Success)
                    {
                        return new XResult<Byte[]>(null, sha1Result.Exceptions.ToArray());
                    }
                    rgbHash = sha1Result.Value;
                    break;
            }

            try
            {
                Byte[] signedData = formatter.CreateSignature(rgbHash);
                return new XResult<Byte[]>(signedData);
            }
            catch (Exception ex)
            {
                return new XResult<Byte[]>(null, ex);
            }
        }

        public static XResult<Boolean> VerifySign(String signNeedToVerify, IDictionary<String, String> paraDic, String publicKey, String signType = "RSA2")
        {
            if (paraDic == null || paraDic.Count == 0)
            {
                return new XResult<Boolean>(false, new ArgumentNullException("paraDic is null"));
            }

            if (String.IsNullOrWhiteSpace(publicKey))
            {
                return new XResult<Boolean>(false, new ArgumentNullException("publicKey is null"));
            }

            return CryptoHelper.VerifySign(signNeedToVerify, GetSignContent(paraDic), publicKey, GetHasAlgName(signType));
        }

        public static XResult<Boolean> VerifySign(String signNeedToVerify, String signContent, String publicKey, String signType = "RSA2")
        {
            return CryptoHelper.VerifySign(signNeedToVerify, signContent, publicKey, GetHasAlgName(signType));
        }

        public static XResult<Boolean> VerifySign(Byte[] sign, Byte[] originalData, String publicKey, String signType = "RSA")
        {
            return CryptoHelper.VerifySign(sign, originalData, publicKey, GetHasAlgName(signType));
        }

        private static String GetSignContent(IDictionary<String, String> paraDic)
        {
            var sortedDic = new SortedDictionary<String, String>(paraDic);
            return String.Join("&", from t0 in sortedDic
                                    where String.Compare(t0.Key, "sign", true) != 0
                                    && !String.IsNullOrWhiteSpace(t0.Value)
                                    select $"{t0.Key}={t0.Value}");
        }

        private static HashAlgorithmName GetHasAlgName(String signType)
        {
            HashAlgorithmName algName = HashAlgorithmName.SHA256;

            if (String.Compare(signType, "RSA", true) == 0)
            {
                algName = HashAlgorithmName.SHA1;
            }
            else if (String.Compare(signType, "MD5", true) == 0)
            {
                algName = HashAlgorithmName.MD5;
            }

            return algName;
        }
    }
}
