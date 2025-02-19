using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace circular.buffer.parallel.benchmark;

public class RabbitMqRunner
{
    private readonly IConnection _connection;
    private readonly IChannel _singletonChannel;
    private readonly UnmanagedCircularBuffer<IChannel> _buffer;
    private readonly object LockObj = new();

    public RabbitMqRunner()
    {
        _connection = ConnectionBuilder().GetAwaiter().GetResult();
        _singletonChannel = _connection.CreateChannelAsync().GetAwaiter().GetResult();
        _buffer = new(10, () => _connection.CreateChannelAsync().GetAwaiter().GetResult(), p => p.IsClosed);
    }

    public async Task CircularBuffer()
    {
        using var buffer = _buffer.Acquire();
        var properties = new BasicProperties();
        var message = Message.GetBytes();
        await buffer.Item.BasicPublishAsync(exchange: string.Empty, routingKey: "teste.queue.1", true, properties, message);
        Thread.Sleep(1000);

    }

    public async Task TransientIModel()
    {
        using var channel = await _connection.CreateChannelAsync();
        var properties = new BasicProperties();
        var message = Message.GetBytes();
        await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "teste.queue.2", true, properties, message);
        Thread.Sleep(1000);
    }

    public async Task SingletonIModel()
    {
        var channel = _singletonChannel;
        var properties = new BasicProperties();
        var message = Message.GetBytes();
        await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "teste.queue.3", true, properties, message);
    }

    // public async Task LockableSingletonIModel()
    // {
    //     lock (LockObj)
    //     {
    //         var channel = _singletonChannel;
    //         var properties = new BasicProperties();
    //         var message = Message.GetBytes();
    //         await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "teste.queue.2", true, properties, message);
    //         Thread.Sleep(1000);
    //     }
    // }

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
