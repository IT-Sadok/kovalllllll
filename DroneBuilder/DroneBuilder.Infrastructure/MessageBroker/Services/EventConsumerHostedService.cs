using System.Text;
using System.Text.Json;
using DroneBuilder.Application.Abstractions;
using DroneBuilder.Application.Models.NotificationModels;
using DroneBuilder.Infrastructure.MessageBroker.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace DroneBuilder.Infrastructure.MessageBroker.Services;

public class EventConsumerHostedService(
    IServiceProvider serviceProvider,
    RabbitMqConfiguration settings,
    EventHandlerRegistry eventHandlerRegistry,
    ILogger<EventConsumerHostedService> logger) : BackgroundService
{
    private IConnection? _connection;
    private IChannel? _channel;

    private readonly List<string> _queuesToListen =
    [
        "user-signed-up-queue"
    ];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await InitializeRabbitMqAsync(stoppingToken);
            await StartConsumingFromAllQueuesAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error starting consumer");
        }
    }

    private async Task InitializeRabbitMqAsync(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = settings.HostName,
            Port = settings.Port,
            UserName = settings.UserName,
            Password = settings.Password,
            VirtualHost = settings.VirtualHost,
            AutomaticRecoveryEnabled = true
        };

        _connection = await factory.CreateConnectionAsync(cancellationToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

        foreach (var queueName in _queuesToListen)
        {
            await _channel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                cancellationToken: cancellationToken
            );
        }

        logger.LogInformation("Consumer initialized for {Count} queues", _queuesToListen.Count);
    }

    private async Task StartConsumingFromAllQueuesAsync(CancellationToken cancellationToken)
    {
        if (_channel == null)
            throw new InvalidOperationException("RabbitMQ channel is not initialized");

        foreach (var queueName in _queuesToListen)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (sender, eventArgs) =>
            {
                await HandleEventAsync(queueName, eventArgs, cancellationToken);
            };

            await _channel.BasicConsumeAsync(
                queue: queueName,
                autoAck: false,
                consumer: consumer,
                cancellationToken: cancellationToken
            );

            logger.LogInformation("Consumer listening on queue: {Queue}", queueName);
        }

        await Task.Delay(Timeout.Infinite, cancellationToken);
    }

    private async Task HandleEventAsync(string queueName, BasicDeliverEventArgs eventArgs,
        CancellationToken cancellationToken)
    {
        try
        {
            var body = eventArgs.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);

            logger.LogInformation("Event received from queue '{Queue}'", queueName);

            var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("type", out var typeProperty))
            {
                logger.LogWarning("Event payload missing 'type' property");
                await _channel!.BasicNackAsync(eventArgs.DeliveryTag, false, false, cancellationToken);
                return;
            }

            var eventType = typeProperty.GetString();
            if (string.IsNullOrEmpty(eventType))
            {
                logger.LogWarning("Event type is null or empty");
                await _channel!.BasicNackAsync(eventArgs.DeliveryTag, false, false, cancellationToken);
                return;
            }

            logger.LogInformation("Event type: {EventType}", eventType);

            using var scope = serviceProvider.CreateScope();

            if (eventHandlerRegistry.CanHandle(eventType))
            {
                await eventHandlerRegistry.HandleAsync(eventType, json, scope, cancellationToken);
                logger.LogInformation("Event processed: {EventType}", eventType);
            }
            else
            {
                logger.LogWarning("No handler registered for event type: {EventType}", eventType);
            }

            await _channel!.BasicAckAsync(eventArgs.DeliveryTag, false, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing event from queue '{Queue}'", queueName);
            await _channel!.BasicNackAsync(eventArgs.DeliveryTag, false, false, cancellationToken);
        }
    }

    public override async void Dispose()
    {
        try
        {
            if (_channel != null) await _channel.CloseAsync();
            if (_connection != null) await _connection.CloseAsync();
            logger.LogInformation("Consumer disposed");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error disposing Consumer");
        }

        base.Dispose();
    }
}