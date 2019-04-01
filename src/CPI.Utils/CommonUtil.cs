using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using ATBase.Core;

namespace CPI.Utils
{
    public static class CommonUtil
    {
        public static XResult<Byte[]> Gzip(Byte[] unzipData)
        {
            if (unzipData == null || unzipData.Length == 0)
            {
                return new XResult<Byte[]>(null, new ArgumentNullException(nameof(unzipData)));
            }

            using (var ms = new MemoryStream())
            {
                using (var gs = new GZipStream(ms, CompressionMode.Compress, true))
                {
                    gs.Write(unzipData, 0, unzipData.Length);
                }

                return new XResult<Byte[]>(ms.ToArray());
            }
        }

        public static XResult<Byte[]> UnZip(Byte[] zipedData)
        {
            MemoryStream ms = null;
            GZipStream gz = null;
            try
            {
                ms = new MemoryStream(zipedData);
                gz = new GZipStream(ms, CompressionMode.Decompress);
                gz.Flush();

                Int32 nSize = 6000 * 1024 + 256;
                Byte[] decompressBuffer = new Byte[nSize];
                Int32 nSizeIncept = gz.Read(decompressBuffer, 0, nSize);
                var ret = new Byte[nSizeIncept];
                Array.Copy(decompressBuffer, ret, ret.Length);
                return new XResult<Byte[]>(ret);
            }
            catch (Exception ex)
            {
                return new XResult<Byte[]>(null, ex);
            }
        }

        public static Dictionary<String, String> ToDictionary(Object value, Boolean declaredOnly = false)
        {
            if (value == null)
            {
                return new Dictionary<String, String>(0);
            }

            var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty;
            if (declaredOnly)
            {
                flags |= BindingFlags.DeclaredOnly;
            }

            var properties = value.GetType().GetProperties(flags);
            if (properties.Length == 0)
            {
                return new Dictionary<String, String>(0);
            }

            var dic = new Dictionary<String, String>(properties.Length);
            foreach (var property in properties)
            {
                dic[property.Name] = (property.XGetValue(value) ?? String.Empty).ToString();
            }

            return dic;
        }
    }
}
