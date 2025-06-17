// TFMS.Data/EnumExtensions.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace TFMS.Models
{
    public static class EnumExtensions
    {
        public static string GetDescription<TEnum>(this TEnum enumeration) where TEnum : Enum
        {
            Type type = enumeration.GetType();
            MemberInfo[] memInfo = type.GetMember(enumeration.ToString());
            if (memInfo != null && memInfo.Length > 0)
            {
                object[] attrs = memInfo[0].GetCustomAttributes(typeof(DisplayAttribute), false);
                if (attrs != null && attrs.Length > 0)
                    return ((DisplayAttribute)attrs[0]).Name ?? enumeration.ToString();
            }
            return enumeration.ToString();
        }
    }
}
