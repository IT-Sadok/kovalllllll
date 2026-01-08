using System.Text;
using System.Text.Json;
using DroneBuilder.Application.Models.NotificationModels;
using DroneBuilder.Infrastructure.MessageBroker.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace DroneBuilder.Infrastructure.MessageBroker.Services;

public class OutboxProcessorHostedService(
    IServiceProvider serviceProvider,
    RabbitMqConfiguration settings,
    ILogger<OutboxProcessorHostedService> logger) : BackgroundService
{
    private IConnection? _connection;
    private IChannel? _channel;

    protected override async Task ExecuteAsync(CancellationToken canceletionToken)
    {
        await InitializeRabbitMqAsync(canceletionToken);

        while (!canceletionToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(canceletionToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing outbox messages");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), canceletionToken);
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

        await _channel.ExchangeDeclareAsync(
            exchange: settings.NotificationExchange,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken
        );

        await _channel.QueueDeclareAsync(
            queue: settings.NotificationQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken
        );

        await _channel.QueueBindAsync(
            queue: settings.NotificationQueueName,
            exchange: settings.NotificationExchange,
            routingKey: settings.NotificationRoutingKey,
            cancellationToken: cancellationToken
        );

        logger.LogInformation(
            "RabbitMQ initialized: Exchange={Exchange}, Queue={Queue}, RoutingKey={RoutingKey}",
            settings.NotificationExchange,
            settings.NotificationQueueName,
            settings.NotificationRoutingKey
        );
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var messages = await context.Messages
            .Where(m => m.ProcessedAt == null && m.RetryCount < 3)
            .OrderBy(m => m.CreatedAt)
            .Take(10)
            .ToListAsync(cancellationToken);

        if (messages.Count == 0)
            return;

        logger.LogInformation("Processing {Count} outbox messages", messages.Count);

        foreach (var message in messages)
        {
            try
            {
                var notification = JsonSerializer.Deserialize<NotificationMessageModel>(message.Payload);
                if (notification == null) continue;
                await PublishToRabbitMqAsync(notification, cancellationToken);
                message.ProcessedAt = DateTime.UtcNow;

                logger.LogInformation(
                    "Notification sent: {Type} - {Title} for user {UserId}",
                    notification.Type,
                    notification.Title,
                    notification.UserId
                );
            }
            catch (Exception ex)
            {
                message.RetryCount++;
                message.Error = ex.Message;
                logger.LogError(ex, "Failed to process outbox message {Id}", message.Id);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task PublishToRabbitMqAsync(NotificationMessageModel notification,
        CancellationToken cancellationToken)
    {
        if (_channel == null)
            throw new InvalidOperationException("RabbitMQ channel is not initialized");

        var json = JsonSerializer.Serialize(notification);
        var body = Encoding.UTF8.GetBytes(json);

        var properties = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json",
            MessageId = notification.Id
        };

        await _channel.BasicPublishAsync(
            exchange: settings.NotificationExchange,
            routingKey: settings.NotificationRoutingKey,
            mandatory: false,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken
        );
    }

    public override async void Dispose()
    {
        try
        {
            if (_channel != null) await _channel.CloseAsync();
            if (_connection != null) await _connection.CloseAsync();
            logger.LogInformation("OutboxProcessorHostedService disposed");
            base.Dispose();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error disposing OutboxProcessorHostedService");
        }
    }
}