using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;
using System.ComponentModel;

namespace CPI.Common
{
    /// <summary>
    /// 
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// 获取枚举类的描述信息
        /// </summary>
        /// <param name="eo"></param>
        public static String GetDescription(this Enum eo)
        {
            var descAttr = eo.GetType().GetField(eo.ToString()).GetCustomAttribute<DescriptionAttribute>();

            if (descAttr != null)
            {
                return descAttr.Description;
            }

            return eo.ToString();
        }
    }
}
