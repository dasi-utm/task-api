using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using TaskManager.Application.Interfaces;

namespace TaskManager.Infrastructure.Services;

public class EventPublisher : IEventPublisher, IAsyncDisposable
{
    private readonly ILogger<EventPublisher> _logger;
    private readonly string _exchange;
    private readonly string _hostName;
    private readonly int _port;
    private readonly string _userName;
    private readonly string _password;
    private IConnection? _connection;
    private IChannel? _channel;

    public EventPublisher(IConfiguration configuration, ILogger<EventPublisher> logger)
    {
        _logger = logger;
        _exchange = configuration["RabbitMQ:Exchange"] ?? "task-events";
        _hostName = configuration["RabbitMQ:HostName"] ?? "localhost";
        _port = int.Parse(configuration["RabbitMQ:Port"] ?? "5672");
        _userName = configuration["RabbitMQ:UserName"] ?? "guest";
        _password = configuration["RabbitMQ:Password"] ?? "guest";
    }

    public async Task PublishAsync(string routingKey, object message)
    {
        try
        {
            await EnsureConnectionAsync();

            var json = JsonSerializer.Serialize(message, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var body = Encoding.UTF8.GetBytes(json);
            var properties = new BasicProperties
            {
                ContentType = "application/json",
                DeliveryMode = DeliveryModes.Persistent
            };

            await _channel!.BasicPublishAsync(
                exchange: _exchange,
                routingKey: routingKey,
                mandatory: false,
                basicProperties: properties,
                body: body);

            _logger.LogInformation(
                "Published event to {Exchange} with routing key {RoutingKey}",
                _exchange, routingKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to publish event with routing key {RoutingKey}", routingKey);
            throw;
        }
    }

    private async Task EnsureConnectionAsync()
    {
        if (_connection is { IsOpen: true } && _channel is { IsOpen: true })
            return;

        var factory = new ConnectionFactory
        {
            HostName = _hostName,
            Port = _port,
            UserName = _userName,
            Password = _password
        };

        _connection = await factory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();

        await _channel.ExchangeDeclareAsync(
            exchange: _exchange,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false);
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
        {
            await _channel.CloseAsync();
            _channel.Dispose();
        }
        if (_connection is not null)
        {
            await _connection.CloseAsync();
            _connection.Dispose();
        }
    }
}
