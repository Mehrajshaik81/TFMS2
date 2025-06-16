// Models/EnumExtensions.cs (or Extensions/EnumExtensions.cs)
using System.ComponentModel;
using System.Reflection;

namespace TFMS.Models // Or TFMS.Extensions, ensure it's accessible
{
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum enumValue)
        {
            FieldInfo? field = enumValue.GetType().GetField(enumValue.ToString());
            if (field == null) return enumValue.ToString(); // Fallback to string name if no field

            DescriptionAttribute[]? attributes = (DescriptionAttribute[]?)field.GetCustomAttributes(typeof(DescriptionAttribute), false);

            return attributes != null && attributes.Length > 0 ? attributes[0].Description : enumValue.ToString();
        }
    }
}