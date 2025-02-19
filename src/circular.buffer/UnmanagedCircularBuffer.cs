namespace circular.buffer;

/// <summary>
/// Buffer that contains unmanaged resources that need to be disposed
/// like file handles, network sockets, etc.
/// </summary>
/// <typeparam name="TObject"></typeparam>
public class UnmanagedCircularBuffer<TObject> : CircularBuffer<TObject>, IDisposable where TObject : IDisposable
{
    private bool disposedValue;

    public UnmanagedCircularBuffer(int capacity, Func<TObject> itemFactory, Func<TObject, bool>? itemInvalidation) : base(capacity, itemFactory, itemInvalidation)
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