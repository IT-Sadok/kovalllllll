using System.Text;
using System.Text.Json;
using DroneBuilder.Application.Abstractions;
using DroneBuilder.Application.Options;
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
    MessageQueuesConfiguration queuesConfig,
    ILogger<EventConsumerHostedService> logger) : BackgroundService
{
    private IConnection? _connection;
    private IChannel? _channel;

    private List<QueueConfiguration> GetQueuesToListen()
    => [
        queuesConfig.UserQueue,
        queuesConfig.CartQueue,
        queuesConfig.OrderQueue,
        queuesConfig.ImageQueue,
        queuesConfig.ProductQueue,
        queuesConfig.WarehouseQueue
    ];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await InitializeRabbitMqAsync(stoppingToken);
                await StartConsumingFromAllQueuesAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "RabbitMQ consumer failed; reconnecting");
                await CloseRabbitMqAsync(CancellationToken.None);
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private async Task InitializeRabbitMqAsync(CancellationToken cancellationToken)
    {
        ConnectionFactory factory;

        if (!string.IsNullOrEmpty(settings.ConnectionString))
        {
            factory = new ConnectionFactory
            {
                Uri = new Uri(settings.ConnectionString),
                AutomaticRecoveryEnabled = true,
                Ssl = new SslOption
                {
                    Enabled = true,
                    ServerName = new Uri(settings.ConnectionString).Host
                }
            };
        }
        else
        {
            factory = new ConnectionFactory
            {
                HostName = settings.HostName,
                Port = settings.Port,
                UserName = settings.UserName,
                Password = settings.Password,
                VirtualHost = settings.VirtualHost,
                AutomaticRecoveryEnabled = true
            };
        }

        _connection = await factory.CreateConnectionAsync(cancellationToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

        List<QueueConfiguration> queuesToListen = GetQueuesToListen();

        foreach (QueueConfiguration queueConfig in queuesToListen)
        {
            await _channel.QueueDeclareAsync(
                queue: queueConfig.Name,
                durable: queueConfig.Durable,
                exclusive: queueConfig.Exclusive,
                autoDelete: queueConfig.AutoDelete,
                arguments: queueConfig.Arguments,
                cancellationToken: cancellationToken
            );

            await _channel.BasicQosAsync(
                prefetchSize: 0,
                prefetchCount: (ushort)queueConfig.PrefetchCount,
                global: false,
                cancellationToken: cancellationToken
            );
        }

        logger.LogInformation("Consumer initialized for {Count} queues", queuesToListen.Count);
    }

    private async Task StartConsumingFromAllQueuesAsync(CancellationToken cancellationToken)
    {
        if (_channel == null)
        {
            throw new InvalidOperationException("RabbitMQ channel is not initialized");
        }

        List<QueueConfiguration> queuesToListen = GetQueuesToListen();

        foreach (QueueConfiguration queueConfig in queuesToListen)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (sender, eventArgs) => await HandleEventAsync(queueConfig, eventArgs, cancellationToken);

            await _channel.BasicConsumeAsync(
                queue: queueConfig.Name,
                autoAck: false,
                consumer: consumer,
                cancellationToken: cancellationToken
            );

            logger.LogInformation("Consumer listening on queue: {Queue} (Prefetch: {PrefetchCount})",
                queueConfig.Name, queueConfig.PrefetchCount);
        }

        await Task.Delay(Timeout.Infinite, cancellationToken);
    }

    private async Task HandleEventAsync(QueueConfiguration queueConfig, BasicDeliverEventArgs eventArgs,
        CancellationToken cancellationToken)
    {
        try
        {
            byte[] body = eventArgs.Body.ToArray();
            string json = Encoding.UTF8.GetString(body);

            logger.LogInformation("Event received from queue '{Queue}'", queueConfig.Name);

            string? eventType = ExtractEventType(json);
            if (eventType == null)
            {
                await _channel!.BasicNackAsync(eventArgs.DeliveryTag, false, false, cancellationToken);
                return;
            }

            logger.LogInformation("Event type: {EventType}", eventType);

            using IServiceScope scope = serviceProvider.CreateScope();

            IEnumerable<IEventHandler> handlers = scope.ServiceProvider.GetServices<IEventHandler>();
            IEventHandler? handler = handlers.FirstOrDefault(h => h.EventType == eventType);

            if (handler == null)
            {
                logger.LogWarning("No handler found for event type: {EventType}", eventType);
                await _channel!.BasicAckAsync(eventArgs.DeliveryTag, false, cancellationToken);
                return;
            }

            await handler.HandleAsync(json, cancellationToken);

            logger.LogInformation("Event processed: {EventType}", eventType);

            await _channel!.BasicAckAsync(eventArgs.DeliveryTag, false, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing event from queue '{Queue}'", queueConfig.Name);
            await _channel!.BasicNackAsync(eventArgs.DeliveryTag, false, false, cancellationToken);
        }
    }

    private string? ExtractEventType(string json)
    {
        try
        {
            var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("type", out JsonElement typeProperty))
            {
                logger.LogWarning("Event payload missing 'type' property");
                return null;
            }

            string? eventType = typeProperty.GetString();
            if (!string.IsNullOrEmpty(eventType))
            {
                return eventType;
            }

            logger.LogWarning("Event type is null or empty");

            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error extracting event type");
            return null;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);
        await CloseRabbitMqAsync(cancellationToken);
    }

    private async Task CloseRabbitMqAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (_channel is not null)
            {
                await _channel.CloseAsync(cancellationToken);
                await _channel.DisposeAsync();
                _channel = null;
            }

            if (_connection is not null)
            {
                await _connection.CloseAsync(cancellationToken);
                await _connection.DisposeAsync();
                _connection = null;
            }
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Error while closing RabbitMQ consumer resources");
        }
    }
}
