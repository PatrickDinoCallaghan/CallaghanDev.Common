using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallaghanDev.Utilities.Math
{
    public class NumberTheory
    {
        /// <summary>
        /// Finds all prime numbers in the specified range and saves them to a file.
        /// </summary>
        /// <remarks>
        /// <para><b>Details:</b></para>
        /// <para>This method uses a segmented sieve approach to find primes in the range [start, end]. It first computes small primes up to the square root of the end using the SimpleSieve method. It then marks non-prime numbers in segments using these small primes, parallelizing the marking process for efficiency. Finally, it saves the found primes to a file.</para>
        /// 
        /// <para><b>Design Choices:</b></para>
        /// <para>Parallelization improves performance by dividing the marking work across multiple threads. Saving the primes to a file ensures persistence.</para>
        /// 
        /// <para><b>TL;DR Design Choices:</b></para>
        /// <para>Uses segmentation and parallel processing to efficiently find and save primes in a range.</para>
        /// </remarks>
        /// <param name="start">The starting number of the range.</param>
        /// <param name="end">The ending number of the range.</param>
        /// <param name="FileName">The name of the file where primes will be saved.</param>
        /// <param name="Sorted">Optional. Whether to sort the primes before saving. Default is false.</param>
        public List<int> FindPrimes(int start, int end, bool Sorted = false)
        {
            List<int> primes = new List<int>() { 2 }; // Add two because it wont be added by the algorithm

            try
            {
                // For any composite number n, there exists at least one divisor p such that p <= n^(1/2)
                // The means we only need to find the sqroot 
                int sqrtEnd = (int)System.Math.Sqrt(end);

                // Get small primes up to sqrt(end)
                List<int> smallPrimes = SimpleSieve(sqrtEnd);

                // Ensure the start is odd and adjust if necessary
                if (start <= 2)
                {
                    if (start == 2)
                    {
                        primes.Add(2);
                    }
                    else
                    {
                        start = 3;
                    }
                }
                // If the starting value of the range is even, you know it mustn't be a prime.
                else if (start % 2 == 0)
                {
                    start++;
                }

                // Segment size can be adjusted; we use a smaller segment size for large ranges
                int segmentSize = System.Math.Max(sqrtEnd, 32768); // Ensure segment size is reasonable
                                                            // Why 32768?
                                                            // - Needs to be power of 2 Segment sizes that are powers of two can be handled more efficiently in memory allocation and addressing.
                                                            // - UInt16 size sounded good, but adjust as needed

                // Initializes the lower bound of the current segment to the start of the range
                int low = start;

                // finds and sets the upper bound of the current segment, ensuring it does not exceed the end of the range
                int high = System.Math.Min(start + segmentSize, end);


                Console.WriteLine("Please wait.");

                // While loop to ensure that the algorithm processes each segment of the range [start, end] sequentially 
                while (low <= end)
                {
                    if (high > end)
                    {
                        high = end;
                    }

                    bool[] mark = new bool[(high - low) / 2 + 1]; // We can do that because we are only interested in evaluating odds, again we know evens cannot be prime.

                    try
                    {
                        Parallel.ForEach(smallPrimes, prime =>
                        {

                            //The goal is to mark multiples of the current prime number as non-prime within the current segment [low, high].
                            //To do this efficiently, we need to find the smallest multiple of the prime number that lies within this segment.

                            // this finds the minimum number in the current segment that is a multiple of prime
                            int startIdx = System.Math.Max(prime * prime, (low + prime - 1) / prime * prime);

                            // Why prime * prime?
                            //This is the smallest multiple of the prime number that needs to be checked. It ensures that we start marking from the square of the prime, as all smaller multiples of this prime have already been marked in previous segments.

                            // Ensure startIdx is odd -< we are only checking odds as we know evens are not prime
                            if (startIdx % 2 == 0)
                            {
                                startIdx += prime;
                            }

                            // Mark multiples of prime in the segment
                            for (int j = startIdx; j <= high; j += 2 * prime)
                            {
                                if (j >= low && (j - low) % 2 == 0)
                                {
                                    // Only interested in odds
                                    mark[(j - low) / 2] = true;
                                }
                            }
                        });
                    }
                    catch (AggregateException ae)
                    {
                        foreach (var ex in ae.InnerExceptions)
                        {
                            throw new InvalidOperationException($"Error in parallel processing: {ex.Message}", ex);
                        }
                    }

                    //Iterates through each odd number in the segment because even numbers (except 2) cannot be prime.
                    for (int i = low; i <= high; i += 2)
                    {
                        ///Adds the number i to the list of primes if it is 2 or if it is greater than 2 and not marked as non-prime (!mark[(i - low) / 2]
                        if (i == 2 || (i > 2 && !mark[(i - low) / 2]))
                        {
                            primes.Add(i);
                        }
                    }

                    low += segmentSize;
                    high += segmentSize;
                }

                if (Sorted)
                {
                    primes.Sort();
                }

            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"An error occurred: {ex.Message}", ex);
            }
            //Next Line in console, 

            return primes;
        }

        /// <summary>
        /// Finds all prime numbers up to a specified limit using the basic Sieve of Eratosthenes.
        /// </summary>
        /// <remarks>
        /// <para><b>Details:</b></para>
        /// <para>This method uses the simple sieve algorithm to find all primes up to the specified limit. It marks multiples of each prime starting from 2, skipping even numbers for efficiency.</para>
        /// 
        /// <para><b>Design Choices:</b></para>
        /// <para>Skipping even numbers significantly reduces the number of operations. The sieve array is initialized to track odd numbers only.</para>
        /// 
        /// <para><b>TL;DR Design Choices:</b></para>
        /// <para>Simple sieve with even number optimization.</para>
        /// </remarks>
        /// <param name="limit">The upper limit up to which prime numbers are to be found.</param>
        /// <returns>A list of prime numbers up to the specified limit.</returns>
        private List<int> SimpleSieve(int limit)
        {
            List<int> primes = new List<int> { 2 };
            bool[] isPrime = new bool[(limit / 2) + 1];

            // Initialize all entries in isPrime as true (except index 0 which represents number 1, which is not prime)
            for (int i = 1; i < isPrime.Length; i++)
            {
                isPrime[i] = true; // All over values are candedates for possible primes
            }

            for (int i = 1; i < isPrime.Length; i++)
            {
                if (isPrime[i])
                {
                    // We know that the next odd number is prime,
                    // First iteration = 3,
                    // By The Seive of Eratosthenes ensures the next index will represent a prime number
                    int prime = 2 * i + 1;
                    primes.Add(prime);

                    // Mark multiples of the prime as non-prime
                    for (int j = prime * prime; j <= limit; j += 2 * prime)
                    {
                        // Map the number j to the index in the isPrime array
                        isPrime[j / 2] = false;
                    }
                }
            }

            return primes;
        }
    }
}
