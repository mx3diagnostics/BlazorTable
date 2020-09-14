using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace BlazorTable
{
    class EnumerableComparerCaseInsensitive : IComparer<IEnumerable<object>>
    {
        private static EnumerableComparerCaseInsensitive _default = new EnumerableComparerCaseInsensitive();
        public static EnumerableComparerCaseInsensitive Default => _default;

        public int Compare(IEnumerable<object> x, IEnumerable<object> y)
        {
            var obj_comp = Comparer<object>.Default;
            var string_cmp = StringComparer.CurrentCultureIgnoreCase;

            if (x == null) return -1;
            if (y == null) return 1;

            using (var leftIt = x.GetEnumerator())
            using (var rightIt = y.GetEnumerator())
            {
                while (true)
                {
                    bool left = leftIt.MoveNext();
                    bool right = rightIt.MoveNext();

                    if (!(left || right)) return 0;

                    if (!left) return -1;
                    if (!right) return 1;

                    int itemResult;
                    if (leftIt.Current is string && rightIt.Current is string)
                        itemResult = string_cmp.Compare(leftIt.Current, rightIt.Current);
                    else
                        itemResult = obj_comp.Compare(leftIt.Current, rightIt.Current);

                    if (itemResult != 0) return itemResult;
                }
            }
        }
    }

    static class NaturalSortExtensionMethods
    {
        private static Func<string, object> convert = s => s == null ? null : (int.TryParse(s, out int i) ? (object)i : (object)s);
        private static Func<string, IEnumerable<object>> split = s => s == null ? null : Regex.Split(s.Replace(" ", "", StringComparison.CurrentCultureIgnoreCase), "([0-9]+)").Select(convert);

        public static IOrderedEnumerable<T> OrderByNatural<T>(this IEnumerable<T> xs, Func<T, string> selector)
        {
            return xs.OrderBy(
                o => split(selector(o)),
                EnumerableComparerCaseInsensitive.Default);
        }

        public static IOrderedEnumerable<string> OrderByNatural(this IEnumerable<string> xs)
        {
            return xs.OrderBy(
                s => split(s),
                EnumerableComparerCaseInsensitive.Default);
        }

        public static IOrderedEnumerable<T> OrderByNaturalDescending<T>(this IEnumerable<T> xs, Func<T, string> selector)
        {
            return xs.OrderByDescending(
                o => split(selector(o)),
                EnumerableComparerCaseInsensitive.Default);
        }

        public static IOrderedEnumerable<string> OrderByNaturalDescending(this IEnumerable<string> xs)
        {
            return xs.OrderByDescending(
                s => split(s),
                EnumerableComparerCaseInsensitive.Default);
        }

        public static IOrderedQueryable<T> OrderByNatural<T>(this IQueryable<T> xs, Expression<Func<T, object>> selector)
        {
            var func = selector.Compile();
            return xs.OrderBy(
                o => split(func(o).ToStringOrNull()),
                EnumerableComparerCaseInsensitive.Default);
        }

        public static IOrderedQueryable<T> OrderByNaturalDescending<T>(this IQueryable<T> xs, Expression<Func<T, object>> selector)
        {
            var func = selector.Compile();
            return xs.OrderByDescending(
                o => split(func(o).ToStringOrNull()),
                EnumerableComparerCaseInsensitive.Default);
        }

        private static string? ToStringOrNull(this object? o)
        {
            if (o == null) return null;
            return o.ToString();
        }
    }
}
