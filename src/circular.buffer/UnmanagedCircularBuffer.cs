namespace circular.buffer;

/// <summary>
/// Circular Buffer that contains unmanaged resources like database connections,
/// tcp connections, etc.
/// </summary>
/// <typeparam name="T"></typeparam>
public class UnmanagedCircularBuffer<T> : CircularBuffer<T>, IDisposable where T : IDisposable
{
    private bool disposedValue;

    public UnmanagedCircularBuffer(int capacity, Func<T> itemFactory) : base(capacity, itemFactory)
    {
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                foreach (var item in _buffer)
                    item.Dispose();

                _buffer.Clear();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}