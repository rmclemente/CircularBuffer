using System.Text;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using RabbitMQ.Client;

namespace circular.buffer.benchmark;

[MemoryDiagnoser(false)]
public class RabbitMqRunner
{
    private readonly IConnection _connection;
    private readonly IChannel _singletonChannel;
    private readonly UnmanagedCircularBuffer<IChannel> _buffer;
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    public RabbitMqRunner()
    {
        _connection = ConnectionBuilder().GetAwaiter().GetResult();
        _singletonChannel = _connection.CreateChannelAsync().GetAwaiter().GetResult();
        _buffer = new UnmanagedCircularBuffer<IChannel>(10, () => _connection.CreateChannelAsync().GetAwaiter().GetResult(), p => p.IsClosed);
    }

    [Benchmark]
    public async Task CircularBuffer()
    {
        using var buffer = _buffer.Acquire();
        var properties = new BasicProperties();
        var message = Message.GetBytes();
        await buffer.Item.BasicPublishAsync(exchange: string.Empty, routingKey: "teste.queue.1", mandatory: true, basicProperties: properties, body: message);
    }

    [Benchmark]
    public async Task TransientIModel()
    {
        using var channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();
        var properties = new BasicProperties();
        var message = Message.GetBytes();
        await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "teste.queue.2", mandatory: true, basicProperties: properties, body: message);
    }

    [Benchmark]
    public async Task SingletonIModel()
    {
        var channel = _singletonChannel;
        var properties = new BasicProperties();
        var message = Message.GetBytes();
        await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "teste.queue.3", mandatory: true, basicProperties: properties, body: message);
    }

    [Benchmark]
    public async Task LockableSingletonIModel()
    {
        await _semaphore.WaitAsync();
        var channel = _singletonChannel;
        var properties = new BasicProperties();
        var message = Message.GetBytes();
        await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "teste.queue.2", mandatory: true, basicProperties: properties, body: message);
        _semaphore.Release();
    }

    public static async Task<IConnection> ConnectionBuilder()
    {
        var factory = new ConnectionFactory
        {
            HostName = "localhost",
            Port = 5672,
            UserName = "checkout_api",
            Password = "checkout123",
            ClientProvidedName = "CircularBufferBench"
        };

        return await factory.CreateConnectionAsync();
    }

    public class Message
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public string Name { get; private set; } = Guid.NewGuid().ToString();
        public DateTime Created { get; private set; } = DateTime.Now;

        public static Message Create() => new();

        public static byte[] GetBytes() => Encoding.UTF8.GetBytes(JsonSerializer.Serialize(Create()));
    }
}
