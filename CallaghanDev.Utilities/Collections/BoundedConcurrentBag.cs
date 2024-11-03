using System.Collections.Concurrent;

namespace CallaghanDev.Utilities.Collections
{
    public class BoundedConcurrentBag<T>
    {
        private readonly ConcurrentQueue<T> _queue = new();
        private readonly int _maxSize;
        private readonly object _lock = new();

        public BoundedConcurrentBag(int maxSize) => _maxSize = maxSize;

        public int Count => _queue.Count;
        public void Add(T item)
        {
            _queue.Enqueue(item);
            lock (_lock)
                while (_queue.Count > _maxSize)
                    _queue.TryDequeue(out _);
        }

        public T[] ToArray() => _queue.ToArray();
    }

    public static class BoundedConcurrentQueueExtensions
    {
        public static List<T> ToList<T>(this BoundedConcurrentBag<T> queue)
        {
            lock (queue)
            {
                return new List<T>(queue.ToArray());
            }
        }
    }
}