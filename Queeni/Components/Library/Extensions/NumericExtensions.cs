using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Queeni.Components.Library.Extensions
{
    public static class NumericExtensions
    {
        public static long ToLong(this double value)
        {
            return Convert.ToInt64(value);
        }
        public static int ToInt(this string str, int? defaultValue = null)
        {
            int num;
            if (str != null && int.TryParse(str, out num))
                return num;
            if (defaultValue.HasValue)
                return defaultValue.Value;
            return int.MinValue;
        }
    }
}
