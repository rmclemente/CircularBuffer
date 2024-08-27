using System.Collections.Concurrent;
using System.Diagnostics;

namespace circular.buffer;

public class CircularBuffer<T>
{
    protected readonly ConcurrentQueue<T> _buffer;
    private readonly Func<T> _itemFactory;
    public bool IsEmpty => _buffer.IsEmpty;
    public bool IsFull => Capacity == _buffer.Count;
    public int Available => _buffer.Count;
    public int Capacity { get; private set; }

    public CircularBuffer(int capacity, Func<T> itemFactory)
    {
        Capacity = capacity;
        _itemFactory = itemFactory;
        _buffer = new ConcurrentQueue<T>();
        Initialize();
    }

    private void Initialize()
    {
        while (IsFull == false)
            _buffer.Enqueue(_itemFactory());
    }

    public BufferItem Acquire()
    {
        T item;

        while (!_buffer.TryDequeue(out item!))
        {
            Debug.WriteLine("Waiting for '{0}' item to be available...", typeof(T).Name);
            Thread.Sleep(1000);
        }

        return new BufferItem(item, this);
    }

    private void GiveBack(T item)
    {
        _buffer.Enqueue(item);
    }

    public record BufferItem(T Item, CircularBuffer<T> Buffer) : IDisposable
    {
        private readonly CircularBuffer<T> _buffer = Buffer;
        public T Item { get; private init; } = Item;
        public static implicit operator T(BufferItem item) => item.Item;
        public void Dispose()
        {
            _buffer.GiveBack(Item);
            GC.SuppressFinalize(this);
        }
    }
}