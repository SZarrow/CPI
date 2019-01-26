using System;
using System.Text;

namespace CPI.Utils
{
    public static class IDGenerator
    {
        /// <summary>
        /// 生成ID
        /// </summary>
        public static Int64 GenerateID()
        {
            var data = Encoding.UTF8.GetBytes(Guid.NewGuid().ToString("n"));
            return (Int64)BitConverter.ToUInt64(data, 0);
        }
    }
}
