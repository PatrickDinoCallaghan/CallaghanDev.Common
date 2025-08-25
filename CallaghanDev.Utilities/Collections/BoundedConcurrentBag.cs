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
            {

                while (_queue.Count > _maxSize)
                {
                    _queue.TryDequeue(out _);
                }
            }
        }

        public T[] ToArray() => _queue.ToArray();
        public void AddRange(IEnumerable<T> items)
        {
            if (items is null)
            {
                throw new ArgumentNullException(nameof(items));
            } 

            foreach (var item in items)
            {
                _queue.Enqueue(item);
            }

            lock (_lock)
            {
                int excess = _queue.Count - _maxSize;
                for (int i = 0; i < excess; i++)
                {
                    _queue.TryDequeue(out _);
                }
            }
        }
        public T Last()
        {
            if (!Any())
            {
                return default(T);
            }
            var array = _queue.ToArray();
            if (array.Length == 0)
            {
                throw new InvalidOperationException("The bag is empty.");
            }
            return array[^1];
        }
        public bool Any() => !_queue.IsEmpty;
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