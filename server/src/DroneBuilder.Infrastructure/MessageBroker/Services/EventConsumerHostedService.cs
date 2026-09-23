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

    private readonly List<IChannel> _channels = [];

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
        try
        {
            _connection = await RabbitMqConnector.ConnectWithRetryAsync(
                settings, logger, "event consumer", stoppingToken);

            foreach (QueueConfiguration queueConfig in GetQueuesToListen())
            {
                await StartConsumingAsync(queueConfig, stoppingToken);
            }

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Event consumer stopped unexpectedly");
        }
    }

    private async Task StartConsumingAsync(QueueConfiguration queueConfig, CancellationToken cancellationToken)
    {
        IChannel channel = await _connection!.CreateChannelAsync(cancellationToken: cancellationToken);
        _channels.Add(channel);

        await channel.QueueDeclareAsync(
            queue: queueConfig.Name,
            durable: queueConfig.Durable,
            exclusive: queueConfig.Exclusive,
            autoDelete: queueConfig.AutoDelete,
            arguments: queueConfig.Arguments?.ToDictionary(x => x.Key, x => (object?)x.Value),
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: MessageRetryHeader.DeadLetterQueueName(queueConfig.Name),
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        await channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: (ushort)queueConfig.PrefetchCount,
            global: false,
            cancellationToken: cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += (_, eventArgs) =>
            HandleEventAsync(channel, queueConfig, eventArgs, cancellationToken);

        await channel.BasicConsumeAsync(
            queue: queueConfig.Name,
            autoAck: false,
            consumer: consumer,
            cancellationToken: cancellationToken);

        logger.LogInformation("Consumer listening on queue: {Queue} (Prefetch: {PrefetchCount})",
            queueConfig.Name, queueConfig.PrefetchCount);
    }

    private async Task HandleEventAsync(
        IChannel channel,
        QueueConfiguration queueConfig,
        BasicDeliverEventArgs eventArgs,
        CancellationToken cancellationToken)
    {
        string json = Encoding.UTF8.GetString(eventArgs.Body.ToArray());

        try
        {
            string? eventType = ExtractEventType(json);
            if (eventType == null)
            {
                await DeadLetterAsync(channel, queueConfig, eventArgs, "payload has no event type", cancellationToken);
                return;
            }

            using IServiceScope scope = serviceProvider.CreateScope();

            IEnumerable<IEventHandler> handlers = scope.ServiceProvider.GetServices<IEventHandler>();
            IEventHandler? handler = handlers.FirstOrDefault(h => h.EventType == eventType);

            if (handler == null)
            {
                logger.LogWarning("No handler found for event type: {EventType}", eventType);
                await channel.BasicAckAsync(eventArgs.DeliveryTag, false, cancellationToken);
                return;
            }

            await handler.HandleAsync(json, cancellationToken);

            logger.LogInformation("Event processed: {EventType}", eventType);

            await channel.BasicAckAsync(eventArgs.DeliveryTag, false, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing event from queue {Queue}", queueConfig.Name);
            await RetryOrDeadLetterAsync(channel, queueConfig, eventArgs, ex.Message, cancellationToken);
        }
    }

    private async Task RetryOrDeadLetterAsync(
        IChannel channel,
        QueueConfiguration queueConfig,
        BasicDeliverEventArgs eventArgs,
        string reason,
        CancellationToken cancellationToken)
    {
        int retryCount = MessageRetryHeader.Read(eventArgs.BasicProperties.Headers);

        if (retryCount >= queueConfig.MaxRetryCount)
        {
            await DeadLetterAsync(channel, queueConfig, eventArgs,
                $"{reason} (after {retryCount} retries)", cancellationToken);
            return;
        }

        await PublishAsync(channel, queueConfig.Name, eventArgs, retryCount + 1, cancellationToken);
        await channel.BasicAckAsync(eventArgs.DeliveryTag, false, cancellationToken);

        logger.LogWarning("Event from {Queue} requeued, attempt {Attempt} of {Max}",
            queueConfig.Name, retryCount + 1, queueConfig.MaxRetryCount);
    }

    private async Task DeadLetterAsync(
        IChannel channel,
        QueueConfiguration queueConfig,
        BasicDeliverEventArgs eventArgs,
        string reason,
        CancellationToken cancellationToken)
    {
        string deadLetterQueue = MessageRetryHeader.DeadLetterQueueName(queueConfig.Name);

        await PublishAsync(channel, deadLetterQueue, eventArgs,
            MessageRetryHeader.Read(eventArgs.BasicProperties.Headers), cancellationToken);

        await channel.BasicAckAsync(eventArgs.DeliveryTag, false, cancellationToken);

        logger.LogError("Event from {Queue} moved to {DeadLetterQueue}: {Reason}",
            queueConfig.Name, deadLetterQueue, reason);
    }

    private static async Task PublishAsync(
        IChannel channel,
        string queueName,
        BasicDeliverEventArgs eventArgs,
        int retryCount,
        CancellationToken cancellationToken)
    {
        var properties = new BasicProperties
        {
            Persistent = true,
            MessageId = eventArgs.BasicProperties.MessageId,
            ContentType = eventArgs.BasicProperties.ContentType,
            Headers = new Dictionary<string, object?> { [MessageRetryHeader.Name] = retryCount }
        };

        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: queueName,
            mandatory: true,
            basicProperties: properties,
            body: eventArgs.Body.ToArray(),
            cancellationToken: cancellationToken);
    }

    private string? ExtractEventType(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
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

        foreach (IChannel channel in _channels)
        {
            try
            {
                await channel.CloseAsync(cancellationToken);
                await channel.DisposeAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error closing a consumer channel");
            }
        }

        _channels.Clear();

        if (_connection != null)
        {
            try
            {
                await _connection.CloseAsync(cancellationToken);
                await _connection.DisposeAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error closing the consumer connection");
            }
        }

        logger.LogInformation("Event consumer stopped");
    }
}
