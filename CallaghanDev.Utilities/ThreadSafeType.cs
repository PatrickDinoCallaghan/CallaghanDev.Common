using System.Runtime.CompilerServices;

namespace CallaghanDev.Utilities
{
    public class ThreadSafe<T>
    {
        private T _value;
        private readonly object _lock = new object();

        public T Value
        {
            get
            {
                // Fast path for int
                if (typeof(T) == typeof(int))
                {
                    // Safely read the int atomically
                    int current = Interlocked.CompareExchange(
                        ref Unsafe.As<T, int>(ref _value),
                        0,  // dummy
                        0   // dummy
                    );
                    return (T)(object)current;
                }
                // Fast path for long
                else if (typeof(T) == typeof(long))
                {
                    long current = Interlocked.Read(ref Unsafe.As<T, long>(ref _value));
                    return (T)(object)current;
                }
                // Fast path for double
                else if (typeof(T) == typeof(double))
                {
                    double current = Interlocked.CompareExchange(
                        ref Unsafe.As<T, double>(ref _value),
                        0.0, // dummy
                        0.0  // dummy
                    );
                    return (T)(object)current;
                }

                // Fallback for everything else
                lock (_lock)
                {
                    return _value;
                }
            }
            set
            {
                // Fast path for int
                if (typeof(T) == typeof(int))
                {
                    Interlocked.Exchange(
                        ref Unsafe.As<T, int>(ref _value),
                        (int)(object)value!
                    );
                }
                // Fast path for long
                else if (typeof(T) == typeof(long))
                {
                    Interlocked.Exchange(
                        ref Unsafe.As<T, long>(ref _value),
                        (long)(object)value!
                    );
                }
                // Fast path for double
                else if (typeof(T) == typeof(double))
                {
                    Interlocked.Exchange(
                        ref Unsafe.As<T, double>(ref _value),
                        (double)(object)value!
                    );
                }
                // Fallback for everything else
                else
                {
                    lock (_lock)
                    {
                        _value = value;
                    }
                }
            }
        }
    }

}
