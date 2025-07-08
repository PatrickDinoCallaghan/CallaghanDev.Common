
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;


namespace CallaghanDev.Utilities.Collections
{

    public static class DiskCacheExtensions
    {
        /// <summary>
        /// Copies all items from any IEnumerable<T> into a new DiskCachedList<T>.
        /// </summary>
        public static DiskCachedList<T> ToDiskCachedList<T>(
            this IEnumerable<T> source,
            int threshold = 10000,
            string? storagePath = null,
            Func<T, byte[]>? serializer = null,
            Func<byte[], T>? deserializer = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            var dcl = new DiskCachedList<T>(
                storagePath: storagePath,
                threshold: threshold,
                serializer: serializer,
                deserializer: deserializer
            );
            dcl.AddRange(source);
            return dcl;
        }
        public static DiskCachedList<T> LoadFromJsonFile<T>(this DiskCachedList<T> _, string jsonFilePath, int threshold = 100_000)
        {
            return Task.Run(()=> LoadFromJsonFileAsync<T>(jsonFilePath, threshold)).Result;
        }

        public static async Task<DiskCachedList<T>> LoadFromJsonFileAsync<T>(string jsonFilePath, int threshold = 100_000)
        {
            var list = new DiskCachedList<T>(threshold: threshold);

            await using var stream = new FileStream(jsonFilePath, FileMode.Open, FileAccess.Read);
            await foreach (var item in JsonSerializer.DeserializeAsyncEnumerable<T>(stream, new JsonSerializerOptions
            {
                AllowTrailingCommas = true
            }))
            {
                if (item != null)
                {
                    list.Add(item);
                }
            }

            return list;
        }

        public static IEnumerable<T> Where<T>(this DiskCachedList<T> source, Func<T, bool> predicate)
        {
            foreach (var item in source)
            {
                if (predicate(item))
                {
                    yield return item;
                }
            }
        }

        public static IEnumerable<TResult> Select<T, TResult>(this DiskCachedList<T> source, Func<T, TResult> selector)
        {
            foreach (var item in source)
            {
                yield return selector(item);
            }
        }

        public static IEnumerable<T> Take<T>(this DiskCachedList<T> source, int count)
        {
            int i = 0;
            foreach (var item in source)
            {
                if (i++ >= count)
                {
                    yield break;
                }
                else
                {
                    yield return item;
                }
            }
        }

        public static IEnumerable<T> Skip<T>(this DiskCachedList<T> source, int count)
        {
            int i = 0;
            foreach (var item in source)
            {
                if (i++ < count)
                {
                    continue;
                }
                {
                    yield return item;
                }
            }
        }
        public static bool Any<T>(this DiskCachedList<T> source, Func<T, bool>? predicate = null)
        {
            foreach (var item in source)
            {
                if (predicate == null || predicate(item))
                {
                    return true;
                }
            }
            return false;
        }

        public static T First<T>(this DiskCachedList<T> source, Func<T, bool>? predicate = null)
        {
            foreach (var item in source)
            {
                if (predicate == null || predicate(item))
                {
                    return item;
                }
            }
            throw new InvalidOperationException("Sequence contains no matching element");
        }

        public static T? FirstOrDefault<T>(this DiskCachedList<T> source, Func<T, bool>? predicate = null)
        {
            foreach (var item in source)
            {
                if (predicate == null || predicate(item))
                    return item;
            }
            return default;
        }

        public static T Last<T>(this DiskCachedList<T> source, Func<T, bool>? predicate = null)
        {
            T? result = default;
            bool found = false;
            foreach (var item in source)
            {
                if (predicate == null || predicate(item))
                {
                    result = item;
                    found = true;
                }
            }
            if (!found)
            {
                throw new InvalidOperationException("Sequence contains no matching element");
            }
            return result!;
        }

        public static T? LastOrDefault<T>(this DiskCachedList<T> source, Func<T, bool>? predicate = null)
        {
            T? result = default;
            foreach (var item in source)
            {
                if (predicate == null || predicate(item))
                {
                    result = item;
                }
            }
            return result;
        }
    }

    /// <summary>
    /// A memory-efficient, disk-backed list implementation that automatically flushes items to disk
    /// when a specified in-memory threshold is exceeded. Ideal for working with large datasets that 
    /// cannot fit entirely in memory.
    ///
    /// Supports adding, enumerating, indexing, and removing items. Data is automatically persisted
    /// to temporary disk storage and cleaned up upon disposal.
    /// 
    /// Example usage:
    /// <code>
    /// using var list = new DiskCachedList<MyRecord>(threshold: 10_000);
    /// 
    /// // Add a large number of items
    /// for (int i = 0; i < 1_000_000; i++)
    /// {
    ///     list.Add(new MyRecord { Id = i, Name = $"Item {i}" });
    /// }
    ///
    /// // Access items by index
    /// var item = list[123456];
    ///
    /// // Enumerate items
    /// foreach (var record in list)
    /// {
    ///     Console.WriteLine(record.Name);
    /// }
    ///
    /// // Persist to JSON
    /// list.SaveToJsonFile("output.json");
    /// </code>
    /// </summary>
    /// <typeparam name="T">The type of elements stored in the list. Must be serializable.</typeparam>

    public sealed class DiskCachedList<T> : IEnumerable<T>, IDisposable
    {
        private readonly int _threshold;
        private readonly string _storagePath;
        private readonly List<T> _buffer = new List<T>();
        private int _fileCounter = 0;
        private ulong _totalCount = 0;
        private readonly object _lock = new();

        private readonly Func<T, byte[]> _serializer;
        private readonly Func<byte[], T> _deserializer;

        private List<(int FileNumber, long Offset)> _diskIndex = new();

        private static readonly HashSet<string> _usedDirectories = new();
        private static readonly object _globalLock = new();

        private bool _disposed = false;

        public T this[ulong index]
        {
            get
            {
                lock (_lock)
                {
                    if (index >= _totalCount)
                        throw new IndexOutOfRangeException();

                    // Check if it's in memory
                    ulong diskCount = (ulong)_diskIndex.Count;
                    if (index >= diskCount)
                    {
                        return _buffer[(int)(index - diskCount)];
                    }

                    var (fileNum, offset) = _diskIndex[(int)index];
                    var file = Path.Combine(_storagePath, $"part_{fileNum:D5}.bin");

                    using var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
                    using var br = new BinaryReader(fs);

                    fs.Seek(offset, SeekOrigin.Begin);
                    int length = br.ReadInt32();
                    var bytes = br.ReadBytes(length);
                    return _deserializer(bytes);
                }
            }
        }
        private void RebuildIndex()
        {
            _diskIndex.Clear();
            var indexFile = Path.Combine(_storagePath, $"index.bin");
            if (File.Exists(indexFile))
            {
                File.Delete(indexFile);
            }

            for (int i = 0; i < _fileCounter; i++)
            {
                var file = Path.Combine(_storagePath, $"part_{i:D5}.bin");
                if (!File.Exists(file)) continue;

                using var fs = new FileStream(file, FileMode.Open, FileAccess.Read);
                using var br = new BinaryReader(fs);
                using var indexStream = new FileStream(indexFile, FileMode.Append);
                using var indexWriter = new BinaryWriter(indexStream);

                while (fs.Position < fs.Length)
                {
                    long pos = fs.Position;
                    int length = br.ReadInt32();
                    fs.Seek(length, SeekOrigin.Current);

                    indexWriter.Write(i);
                    indexWriter.Write(pos);

                    _diskIndex.Add((i, pos));
                }
            }
            _totalCount = (ulong)_diskIndex.Count + (ulong)_buffer.Count;

        }

        private void LoadIndex()
        {
            var indexPath = Path.Combine(_storagePath, "index.bin");
            if (!File.Exists(indexPath)) return;

            using var fs = new FileStream(indexPath, FileMode.Open, FileAccess.Read);
            using var br = new BinaryReader(fs);

            while (fs.Position < fs.Length)
            {
                int fileNum = br.ReadInt32();
                long offset = br.ReadInt64();
                _diskIndex.Add((fileNum, offset));
            }
        }
        public DiskCachedList(string? storagePath = null, int threshold = 10000, Func<T, byte[]>? serializer = null, Func<byte[], T>? deserializer = null)
        {
            _threshold = threshold;

            _storagePath = InitStoragePath(storagePath);
  
            _serializer = serializer ?? (item => JsonSerializer.SerializeToUtf8Bytes(item));
            _deserializer = deserializer ?? (bytes => JsonSerializer.Deserialize<T>(bytes)!);
            _diskIndex = new List<(int, long)>();

            LoadIndex();
        }

        private string InitStoragePath(string? storagePath)
        {
            if (string.IsNullOrWhiteSpace(storagePath))
            {
                lock(_globalLock)
                {
                    var appBase = AppContext.BaseDirectory;
                    var guidSegment = Guid.NewGuid().ToString("N")[..8];

                    storagePath = Path.Combine(appBase, "DCL", guidSegment);

                    while (_usedDirectories.Contains(storagePath) || Directory.Exists(storagePath))
                    {
                        if (!_usedDirectories.Contains(storagePath) && Directory.Exists(storagePath))
                        {
                            Directory.Delete(storagePath, true);
                            break;
                        }
                        guidSegment = Guid.NewGuid().ToString("N")[..8];
                        storagePath = Path.Combine(appBase, "DCL", guidSegment);
                    }

                    Directory.CreateDirectory(storagePath);
                    _usedDirectories.Add(storagePath);
                }
            }
            return storagePath;

        } 

        public void Add(T item)
        {
            lock (_lock)
            {
                _buffer.Add(item);
                _totalCount++;

                if (_buffer.Count >= _threshold)
                {
                    FlushToDisk();
                }
            }
        }
        public void AddRange(IEnumerable<T> items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));

            lock (_lock)
            {
                foreach (var item in items)
                {
                    _buffer.Add(item);
                    _totalCount++;

                    if (_buffer.Count >= _threshold)
                    {
                        FlushToDisk();
                    }
                }
            }
        }

        public bool Remove(T item)
        {
            lock (_lock)
            {
                // Try remove from buffer
                if (_buffer.Remove(item))
                {
                    _totalCount--;
                    return true;
                }

                for (int i = 0; i < _fileCounter; i++)
                {
                    string file = Path.Combine(_storagePath, $"part_{i:D5}.bin");
                    if (!File.Exists(file)) continue;

                    string tempFile = file + ".tmp";
                    bool found = false;

                    using (var readFs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
                    using (var br = new BinaryReader(readFs))
                    using (var writeFs = new FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.None))
                    using (var bw = new BinaryWriter(writeFs))
                    {
                        while (readFs.Position < readFs.Length)
                        {
                            int length = br.ReadInt32();
                            byte[] bytes = br.ReadBytes(length);
                            T obj = _deserializer(bytes);

                            if (!found && EqualityComparer<T>.Default.Equals(obj, item))
                            {
                                found = true;
                                _totalCount--;
                                continue; // Skip writing this item
                            }

                            bw.Write(length);
                            bw.Write(bytes);
                        }
                    }

                    if (found)
                    {
                        File.Delete(file);
                        File.Move(tempFile, file);
                        RebuildIndex();
                        return true;
                    }
                    else
                    {
                        File.Delete(tempFile); // Clean up
                    }
                }

                return false; // Not found
            }
        }

        public void FlushToDisk()
        {
            lock (_lock)
            {
                if (_buffer.Count == 0)
                {
                    return;

                }

                var fileName = Path.Combine(_storagePath, $"part_{_fileCounter:D5}.bin");
                var indexFile = Path.Combine(_storagePath, $"index.bin");

                using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
                {

                    using (var bw = new BinaryWriter(fs))
                    {
                        using (var indexStream = new FileStream(indexFile, FileMode.Append, FileAccess.Write, FileShare.None))
                        {
                            using (var indexWriter = new BinaryWriter(indexStream))
                            {
                                var newEntries = new List<(int, long)>();

                                foreach (var item in _buffer)
                                {
                                    var pos = fs.Position;
                                    var bytes = _serializer(item);
                                    bw.Write(bytes.Length);
                                    bw.Write(bytes);

                                    indexWriter.Write(_fileCounter);
                                    indexWriter.Write((long)pos);

                                    newEntries.Add((_fileCounter, pos));

                                }
                                _diskIndex.AddRange(newEntries);

                                _fileCounter++;
                                _buffer.Clear();
                            }
                        }

                    }
                }

            }
        }

        public ulong Count()
        {
            return _totalCount;
        }

        #region Disposal and Cleanup

        ~DiskCachedList() => Dispose(false);
        //The intent is to delete all persisted data on disposal. Free up disk space accordingly
        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                lock (_globalLock)
                {
                    _usedDirectories.Remove(_storagePath);

                    if (Directory.Exists(_storagePath))
                    {
                        try
                        {
                            Directory.Delete(_storagePath, recursive: true);
                        }
                        catch (IOException)
                        {
                            // somebody still has a file open.  Ignore and move on.
                        }
                        catch (UnauthorizedAccessException)
                        {
                            // no delete permission, or files are read-only.  Ignore.
                        } 
                    }
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        public void Clear() //Shouldn't delete or removed the directory, object isn't disposed with clear, instead delete all items
        {
            lock (_lock)
            {
                _buffer.Clear();
                _diskIndex.Clear();

                _fileCounter = 0;
                _totalCount = 0;

                foreach (var file in Directory.EnumerateFiles(_storagePath, "part_*.bin"))
                {
                    File.Delete(file);
                }
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            List<(int FileNum, long Offset)> snapshotIndex;
            List<T> snapshotBuffer;

            lock (_lock)
            {
                snapshotIndex = new List<(int, long)>(_diskIndex);
                snapshotBuffer = new List<T>(_buffer);
            }

            foreach (var (fileNum, offset) in snapshotIndex)
            {
                var file = Path.Combine(_storagePath, $"part_{fileNum:D5}.bin");

                using var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
                using var br = new BinaryReader(fs);

                fs.Seek(offset, SeekOrigin.Begin);
                int length = br.ReadInt32();
                var bytes = br.ReadBytes(length);
                yield return _deserializer(bytes);
            }

            foreach (var item in snapshotBuffer)
            {
                yield return item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void SaveToJsonFile( string jsonFilePath, int chunkSize = 1000) //This needs to save to file incrementally
        {
            using var stream = new FileStream(jsonFilePath, FileMode.Create, FileAccess.Write);
            using var writer = new StreamWriter(stream);
            writer.WriteLine("["); // Start JSON array

            int count = 0;
            bool isFirst = true;

            foreach (var item in this)
            {
                if (!isFirst)
                {
                    writer.WriteLine(",");
                }

                var json = JsonSerializer.Serialize(item, new JsonSerializerOptions { WriteIndented = true });
                writer.Write(json);

                isFirst = false;

                count++;
                if (count % chunkSize == 0)
                {
                    writer.Flush();
                }
            }

            writer.WriteLine();
            writer.WriteLine("]");
            writer.Flush();
        }

    }

}
