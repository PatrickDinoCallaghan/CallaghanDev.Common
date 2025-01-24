
using System;
using System.Collections.Generic;

namespace CallaghanDev.Utilities.Extensions
{
    /// <summary>
    /// LINQ-like extension methods for ReadOnlySpan{T}.
    /// Note: To avoid state-machine issues with ref structs,
    /// each method creates and returns a new List instead of using 'yield return'.
    /// </summary>
    public static class ReadOnlySpanExtensions
    {
        /// <summary>
        /// Filters a sequence of values based on a predicate.
        /// </summary>
        public static IEnumerable<T> Where<T>(this ReadOnlySpan<T> span, Func<T, bool> predicate)
        {
            var results = new List<T>(span.Length);
            for (int i = 0; i < span.Length; i++)
            {
                if (predicate(span[i]))
                {
                    results.Add(span[i]);
                }
            }
            return results;
        }

        /// <summary>
        /// Projects each element of a sequence into a new form.
        /// </summary>
        public static IEnumerable<TResult> Select<T, TResult>(this ReadOnlySpan<T> span, Func<T, TResult> selector)
        {
            var results = new List<TResult>(span.Length);
            for (int i = 0; i < span.Length; i++)
            {
                results.Add(selector(span[i]));
            }
            return results;
        }

        /// <summary>
        /// Returns the specified number of contiguous elements from the start of the span.
        /// </summary>
        public static IEnumerable<T> Take<T>(this ReadOnlySpan<T> span, int count)
        {
            count = Math.Min(count, span.Length);
            var results = new List<T>(count);
            for (int i = 0; i < count; i++)
            {
                results.Add(span[i]);
            }
            return results;
        }

        /// <summary>
        /// Bypasses a specified number of elements in a sequence and then returns the remaining elements.
        /// </summary>
        public static IEnumerable<T> Skip<T>(this ReadOnlySpan<T> span, int count)
        {
            var max = Math.Max(0, span.Length - count);
            var results = new List<T>(max);
            for (int i = count; i < span.Length; i++)
            {
                results.Add(span[i]);
            }
            return results;
        }

        /// <summary>
        /// Determines whether any element of a sequence satisfies a condition.
        /// </summary>
        public static bool Any<T>(this ReadOnlySpan<T> span, Func<T, bool> predicate)
        {
            for (int i = 0; i < span.Length; i++)
            {
                if (predicate(span[i]))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Determines whether all elements of a sequence satisfy a condition.
        /// </summary>
        public static bool All<T>(this ReadOnlySpan<T> span, Func<T, bool> predicate)
        {
            for (int i = 0; i < span.Length; i++)
            {
                if (!predicate(span[i]))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Returns the first element in the span that satisfies a specified condition.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if no element satisfies the condition.
        /// </exception>
        public static T First<T>(this ReadOnlySpan<T> span, Func<T, bool> predicate)
        {
            for (int i = 0; i < span.Length; i++)
            {
                if (predicate(span[i]))
                {
                    return span[i];
                }
            }
            throw new InvalidOperationException("Sequence contains no matching element.");
        }

        /// <summary>
        /// Returns the first element in the span that satisfies a specified condition 
        /// or a default value if no such element is found.
        /// </summary>
        public static T FirstOrDefault<T>(this ReadOnlySpan<T> span, Func<T, bool> predicate)
        {
            for (int i = 0; i < span.Length; i++)
            {
                if (predicate(span[i]))
                {
                    return span[i];
                }
            }
            return default!;
        }

        /// <summary>
        /// Counts the number of elements in the span that satisfy a specified condition.
        /// </summary>
        public static int Count<T>(this ReadOnlySpan<T> span, Func<T, bool> predicate)
        {
            int result = 0;
            for (int i = 0; i < span.Length; i++)
            {
                if (predicate(span[i]))
                {
                    result++;
                }
            }
            return result;
        }
    }
}