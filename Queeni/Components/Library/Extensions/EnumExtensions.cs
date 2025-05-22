using Queeni.Data.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Queeni.Components.Library.Extensions
{
    public static class EnumExtensions
    {
        public static IList<string> ToList(this Enum source)
        {
            var t = source.GetType();
            return Enum.GetValues(t).Cast<Enum>().Select(x => x.ToString()).ToList();
        }

        public static Dictionary<int, string> ToDictionary(this Enum source)
        {
            var t = source.GetType();
            return Enum.GetValues(t)
                .Cast<object>()
                .ToDictionary(k => (int)k, v => ((Enum)v).ToStringDescription());
        }

        public static string ToDescriptionOrName(this Enum enumeration)
        {
            var value = enumeration.ToString();
            var enumType = enumeration.GetType();
            var descAttribute = (DescriptionAttribute[])enumType
                .GetField(value)
                .GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (descAttribute.Length > 0)
                return descAttribute[0].Description;

            var displayAttribute = (DisplayAttribute[])enumType
                .GetField(value)
                .GetCustomAttributes(typeof(DisplayAttribute), false);
            if (displayAttribute.Length > 0)
                return displayAttribute[0].Name;

            return value;
        }

        public static string ToStringDescription(this Enum enumeration)
        {
            var value = enumeration.ToString();
            var enumType = enumeration.GetType();
            var descAttribute = (DescriptionAttribute[])enumType
                .GetField(value)
                .GetCustomAttributes(typeof(DescriptionAttribute), false);
            return descAttribute.Length > 0 ? descAttribute[0].Description : value;
        }

        public static string ToStringDisplayName(this Enum enumeration)
        {
            var value = enumeration.ToString();
            var enumType = enumeration.GetType();
            var descAttribute = (DisplayAttribute[])enumType
                .GetField(value)
                .GetCustomAttributes(typeof(DisplayAttribute), false);
            return descAttribute.Length > 0 ? descAttribute[0].Name : value;
        }

        public static string ToStringCategory(this Enum enumeration)
        {
            var value = enumeration.ToString();
            var enumType = enumeration.GetType();
            var descAttribute = (CategoryAttribute[])enumType
                .GetField(value)
                .GetCustomAttributes(typeof(CategoryAttribute), false);
            return descAttribute.Length > 0 ? descAttribute[0].Category : value;
        }

        public static T ParseEnum<T>(this string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        public static T ToEnum<T>(this string value, bool ignoreCase = true)
        {
            return (T)Enum.Parse(typeof(T), value, ignoreCase);
        }

        public static T ToEnum<T>(this int value, bool ignoreCase = true)
        {
            return (T)Enum.ToObject(typeof(T), value);
        }


        public static TAttribute GetAttribute<TAttribute>(this Enum value)
            where TAttribute : System.Attribute
        {
            var enumType = value.GetType();
            var name = Enum.GetName(enumType, value);
            return enumType.GetField(name).GetCustomAttributes(false).OfType<TAttribute>().SingleOrDefault();
        }
    }

    public class EnumDropDownViewModel : EntityModel
    {
        public int Value { get; set; }

        public string Name { get; set; }
    }
}
