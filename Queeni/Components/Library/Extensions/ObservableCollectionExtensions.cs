using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Queeni.Components.Library.Extensions
{
    public static class ObservableCollectionExtensions
    {
        public static void TryRemove<T>(this ObservableCollection<T> source, T value)
        {
            if (source.Contains(value))
                source.Remove(value);
        }

        public static ICollection<T> AddRange<T>(this ICollection<T> source, ICollection<T> items)
        {
            foreach (T item in items)
            {
                source.Add(item);
            }
            return source;
        }
    }
}
