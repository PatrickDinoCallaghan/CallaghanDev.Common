using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallaghanDev.Utilities.Extensions
{
    public static class ParallelExtensions
    {
        public static void For(ulong fromInclusive, ulong toExclusive, ParallelOptions parallelOptions, Action<ulong> body)
        {
            // Determine the number of iterations to split the work into,
            // aiming to stay within the bounds of a long for each chunk.
            // This is a simplistic approach and might need adjustment based on your needs.

            // Assuming the work cannot be split into more than long.MaxValue chunks.
            // This calculation could be adjusted to better suit smaller ranges or different chunking strategies.
            ulong totalWork = toExclusive - fromInclusive;
            ulong maxLongValue = (ulong)long.MaxValue;
            int numberOfChunks = (int)Math.Min((totalWork / maxLongValue) + 1, maxLongValue);

            Task[] tasks = new Task[numberOfChunks];

            for (int chunk = 0; chunk < numberOfChunks; chunk++)
            {
                tasks[chunk] = Task.Factory.StartNew((state) =>
                {
                    int chunkIndex = (int)state;
                    ulong chunkSize = totalWork / (ulong)numberOfChunks;
                    ulong start = fromInclusive + (ulong)chunkIndex * chunkSize;
                    ulong end = (chunkIndex == numberOfChunks - 1) ? toExclusive : start + chunkSize;

                    for (ulong j = start; j < end; j++)
                    {
                        body(j);
                    }
                }, chunk, parallelOptions.CancellationToken, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
            }

            Task.WaitAll(tasks, parallelOptions.CancellationToken);
        }
    }
}
