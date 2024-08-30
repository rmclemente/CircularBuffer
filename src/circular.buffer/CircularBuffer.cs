using System.Collections.Concurrent;
using System.Diagnostics;

namespace circular.buffer;

/// <summary>
/// Buffer that contains managed resources that do not need to be disposed.
/// </summary>
/// <typeparam name="TObject"></typeparam>
public class CircularBuffer<TObject>
{
    /// <summary>
    /// Buffer that contains TObjects.
    /// </summary>
    protected readonly ConcurrentQueue<TObject> _buffer;
    /// <summary>
    /// Function that creates a new TObject.
    /// </summary>
    protected Func<TObject> _itemFactory;
    /// <summary>
    /// Function that invalidates a TObject.
    /// </summary>
    protected readonly Func<TObject, bool>? _itemInvalidation;
    /// <summary>
    /// Returns true if no TObjects are available in buffer.
    /// </summary>
    public bool IsEmpty => _buffer.IsEmpty;
    /// <summary>
    /// Returns true if all TObjects are available in buffer.
    /// </summary>
    public bool IsFull => Capacity == _buffer.Count;
    /// <summary>
    /// Returns the number of available TObjects in buffer.
    /// </summary>
    public int Available => _buffer.Count;
    /// <summary>
    /// Returns the capacity of the buffer.
    /// </summary>
    public int Capacity { get; private set; }

    private CircularBuffer(int capacity, Func<TObject> itemFactory) : this(capacity, itemFactory, null)
    {
    }

    protected CircularBuffer(int capacity, Func<TObject> itemFactory, Func<TObject, bool>? itemInvalidation)
    {
        Capacity = capacity;
        _itemFactory = itemFactory;
        _itemInvalidation = itemInvalidation;
        _buffer = new ConcurrentQueue<TObject>();
        Initialize();
    }

    /// <summary>
    /// Initializes the buffer with TObjects while it is not full.
    /// </summary>
    private void Initialize()
    {
        while (IsFull == false)
            _buffer.Enqueue(_itemFactory());
    }

    public static CircularBuffer<TObject> Create(int capacity, Func<TObject> itemFactory) 
    {
        if (capacity <= 0)
            throw new ArgumentException($"Capacity must be greater than zero. Value: '{capacity}'", nameof(capacity));

        if(itemFactory is null)
            throw new ArgumentNullException(nameof(itemFactory), "ItemFactory can not be null.");

        return new(capacity, itemFactory);
    }

    /// <summary>
    /// Returns a BufferItem that contains a TObject.
    /// </summary>
    /// <returns></returns>
    public BufferItem Acquire()
    {
        TObject item;

        while (!_buffer.TryDequeue(out item!))
        {
            Debug.WriteLine("Waiting for '{0}' item to be available...", typeof(TObject).Name);
            Thread.Sleep(1000);
        }

        var isInvalidated = ItemInvalidationHandler(item);
        if (isInvalidated)
            return new BufferItem(_itemFactory(), this);
        else
            return new BufferItem(item, this);
    }

    /// <summary>
    /// Returns a TObject to the buffer.
    /// </summary>
    private void GiveBack(TObject item)
    {
        var isInvalidated = ItemInvalidationHandler(item);
        if (isInvalidated)
            _buffer.Enqueue(_itemFactory());
        else
            _buffer.Enqueue(item);
    }

    /// <summary>
    /// Checks if a TObject is invalid and disposes it when possible. 
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    private bool ItemInvalidationHandler(TObject item)
    {
        if (_itemInvalidation is not null)
        {
            var isInvalid = _itemInvalidation.Invoke(item);
            if (isInvalid)
            {
                if (item is IDisposable disposableItem)
                    disposableItem?.Dispose();

                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Restarts the buffer with new TObjects using the provided itemFactory.
    /// </summary>
    /// <param name="itemFactory"></param>
    public void RestartBuffer(Func<TObject> itemFactory)
    {
        ArgumentNullException.ThrowIfNull(itemFactory);
        _itemFactory = itemFactory;
        _buffer.Clear();
        Initialize();
    }

    /// <summary>
    /// A wrapper for a TObject that returns it to the buffer when disposed.
    /// </summary>
    /// <param name="Item"></param>
    /// <param name="Buffer"></param>
    public record BufferItem(TObject Item, CircularBuffer<TObject> Buffer) : IDisposable
    {
        private readonly CircularBuffer<TObject> _buffer = Buffer;
        public TObject Item { get; private init; } = Item;
        public static implicit operator TObject(BufferItem item) => item.Item;
        public void Dispose()
        {
            _buffer.GiveBack(Item);
            GC.SuppressFinalize(this);
        }
    }
}