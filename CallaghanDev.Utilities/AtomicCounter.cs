using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallaghanDev.Utilities
{
    public class AtomicCounter
    {
        private int _value;

        public AtomicCounter(int initialValue = 0)
        {
            _value = initialValue;
        }

        /// <summary>
        /// Returns the current value.
        /// </summary>
        public int Value => Volatile.Read(ref _value);

        /// <summary>
        /// Atomically increments the counter and returns the incremented value.
        /// </summary>
        public int Increment() => Interlocked.Increment(ref _value);

        /// <summary>
        /// Atomically decrements the counter and returns the decremented value.
        /// </summary>
        public int Decrement() => Interlocked.Decrement(ref _value);

        /// <summary>
        /// Adds delta to the counter and returns the new value.
        /// </summary>
        public int Add(int delta) => Interlocked.Add(ref _value, delta);

        /// <summary>
        /// Resets the counter to zero and returns the previous value.
        /// </summary>
        public int Reset() => Interlocked.Exchange(ref _value, 0);
    }
}
