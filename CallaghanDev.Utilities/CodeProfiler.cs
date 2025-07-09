using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallaghanDev.Utilities
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;

    namespace Utilities.Diagnostics
    {
        /// <summary>
        /// Profiler that marks code regions and later print a ranked report showing where time was spent.
        /// 
        /// Usage:
        ///   using (CodeProfiler.Step("LoadData"))
        ///   {
        ///       LoadData();
        ///   }
        /// 
        ///   // … after the program (or test) finishes …
        ///   CodeProfiler.PrintReport();
        /// </summary>
        public static class CodeProfiler
        {
            #region Private helpers

            private sealed class Section : IDisposable
            {
                private readonly string _name;
                private readonly Stopwatch _sw;

                internal Section(string name)
                {
                    _name = name;
                    _sw = Stopwatch.StartNew();
                }

                public void Dispose()
                {
                    _sw.Stop();
                    long elapsed = _sw.ElapsedTicks;

                    _aggregates.AddOrUpdate(
                        _name,
                        // First time we see this section
                        _ => new SectionStats(elapsed, 1),
                        // Subsequent times – add the new measurement
                        (_, prev) => prev.Add(elapsed)
                    );
                }
            }

            private readonly struct SectionStats
            {
                public long Ticks { get; }
                public int Count { get; }

                public SectionStats(long ticks, int count)
                {
                    Ticks = ticks;
                    Count = count;
                }

                public SectionStats Add(long extraTicks) =>
                    new SectionStats(Ticks + extraTicks, Count + 1);

                public TimeSpan Elapsed => TimeSpan.FromTicks(Ticks);
            }

            private static readonly ConcurrentDictionary<string, SectionStats> _aggregates = new();

            #endregion

            /// <summary>
            /// Marks the beginning of a profiled region. Prefer the using‑block pattern:
            ///   using (CodeProfiler.Step("MySection")) { /* … */ }
            /// </summary>
            public static IDisposable Step(string name) => new Section(name);

            /// <summary>Convenience wrapper: executes <paramref name="action"/> inside a Step.</summary>
            public static void Profile(string name, Action action)
            {
                using (Step(name))
                {
                    action();
                }
            }

            /// <summary>Generic wrapper that profiles and returns a value.</summary>
            public static T Profile<T>(string name, Func<T> func)
            {
                using (Step(name))
                {
                    return func();
                }
            }

            /// <summary>Clears all collected data.</summary>
            public static void Reset() => _aggregates.Clear();

            /// <summary>
            /// Produces a tab‑separated report sorted by total time (descending). Set <paramref name="top"/>
            /// to limit how many rows are included (0 = all).
            /// </summary>
            public static string GetReport(int top = 0)
            {
                IEnumerable<KeyValuePair<string, SectionStats>> ordered =
                _aggregates.OrderByDescending(kv => kv.Value.Ticks);
                if (top > 0) ordered = ordered.Take(top);

                long totalTicks = _aggregates.Sum(kv => kv.Value.Ticks);
                var sb = new StringBuilder();

                sb.AppendLine("=== CodeProfiler Report ===");
                sb.AppendLine($"Total measured time: {TimeSpan.FromTicks(totalTicks)}");
                sb.AppendLine("Section\tCalls\tTotal\tAvg\tPercent");

                foreach (var kv in ordered)
                {
                    var name = kv.Key;
                    var stats = kv.Value;
                    double pct = totalTicks > 0 ? (double)stats.Ticks / totalTicks * 100.0 : 0.0;

                    sb.AppendLine($"{name}\t{stats.Count}\t{stats.Elapsed}\t{TimeSpan.FromTicks(stats.Ticks / stats.Count)}\t{pct:F2}%");
                }

                return sb.ToString();
            }

            /// <summary>
            /// Writes the report produced by <see cref="GetReport"/> to <see cref="Console.Out"/>.
            /// </summary>
            public static void PrintReport(int top = 0)
            {
                Console.WriteLine(GetReport(top));
            }
        }
    }

}
