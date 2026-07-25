using System.Data;
using System.Text;
using DroneBuilder.Application.Options;
using DroneBuilder.Infrastructure.Data.Entities;
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
    MessageQueuesConfiguration queuesConfig,
    ILogger<OutboxProcessorHostedService> logger) : BackgroundService
{
    private static readonly TimeSpan ProcessingInterval = TimeSpan.FromSeconds(5);
    private IConnection? _connection;
    private IChannel? _channel;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (_channel is null)
                {
                    await InitializeRabbitMqAsync(stoppingToken);
                }

                await ProcessOutboxMessagesAsync(stoppingToken);
                await Task.Delay(ProcessingInterval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Outbox processing failed; reconnecting to RabbitMQ");
                await CloseRabbitMqAsync(CancellationToken.None);
                await Task.Delay(ProcessingInterval, stoppingToken);
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
        var channelOptions = new CreateChannelOptions(
            publisherConfirmationsEnabled: true,
            publisherConfirmationTrackingEnabled: true);
        _channel = await _connection.CreateChannelAsync(channelOptions, cancellationToken);

        logger.LogInformation("RabbitMQ producer initialized with publisher confirmations");
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction = await context.Database.BeginTransactionAsync(
            IsolationLevel.ReadCommitted,
            cancellationToken);

        List<OutboxMessage> messages = await context.Messages
            .FromSqlInterpolated($$"""
                SELECT *
                FROM "Messages"
                WHERE "ProcessedAt" IS NULL
                  AND (
                    ("QueueName" = {{queuesConfig.UserQueue.Name}} AND "RetryCount" < {{queuesConfig.UserQueue.MaxRetryCount}})
                    OR ("QueueName" = {{queuesConfig.CartQueue.Name}} AND "RetryCount" < {{queuesConfig.CartQueue.MaxRetryCount}})
                    OR ("QueueName" = {{queuesConfig.OrderQueue.Name}} AND "RetryCount" < {{queuesConfig.OrderQueue.MaxRetryCount}})
                    OR ("QueueName" = {{queuesConfig.ImageQueue.Name}} AND "RetryCount" < {{queuesConfig.ImageQueue.MaxRetryCount}})
                    OR ("QueueName" = {{queuesConfig.ProductQueue.Name}} AND "RetryCount" < {{queuesConfig.ProductQueue.MaxRetryCount}})
                    OR ("QueueName" = {{queuesConfig.WarehouseQueue.Name}} AND "RetryCount" < {{queuesConfig.WarehouseQueue.MaxRetryCount}})
                  )
                ORDER BY "CreatedAt"
                FOR UPDATE SKIP LOCKED
                LIMIT 10
                """)
            .ToListAsync(cancellationToken);

        if (messages.Count == 0)
        {
            await transaction.CommitAsync(cancellationToken);
            return;
        }

        logger.LogInformation("Processing {Count} outbox messages", messages.Count);

        foreach (OutboxMessage message in messages)
        {
            try
            {
                if (_channel is null)
                {
                    throw new InvalidOperationException("RabbitMQ channel is not initialized.");
                }

                byte[] body = Encoding.UTF8.GetBytes(message.Payload);
                await _channel.BasicPublishAsync(
                    exchange: string.Empty,
                    routingKey: message.QueueName,
                    mandatory: true,
                    basicProperties: new BasicProperties
                    {
                        Persistent = true,
                        MessageId = message.Id,
                        ContentType = "application/json"
                    },
                    body: body,
                    cancellationToken: cancellationToken);

                message.ProcessedAt = DateTime.UtcNow;
                message.Error = null;
                logger.LogInformation(
                    "Event {EventType} published to queue '{QueueName}' (MessageId: {MessageId})",
                    message.Type,
                    message.QueueName,
                    message.Id);
            }
            catch (Exception exception)
            {
                message.RetryCount++;
                message.Error = exception.Message;
                logger.LogError(
                    exception,
                    "Failed to publish message {MessageId} to queue '{QueueName}'",
                    message.Id,
                    message.QueueName);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
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
            logger.LogWarning(exception, "Error while closing RabbitMQ producer resources");
        }
    }
}
