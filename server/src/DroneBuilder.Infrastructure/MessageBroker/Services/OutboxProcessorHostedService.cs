using System.Text;
using DroneBuilder.Domain.Entities;
using DroneBuilder.Infrastructure.MessageBroker.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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
    private const int BatchSize = 50;
    private const int MaxPublishAttempts = 3;

    private static readonly TimeSpan IdleDelay = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan StuckMessageLogInterval = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan PurgeInterval = TimeSpan.FromHours(1);
    private static readonly TimeSpan ProcessedMessageRetention = TimeSpan.FromDays(7);

    private IConnection? _connection;
    private IChannel? _channel;
    private DateTime _lastStuckMessageLogUtc = DateTime.MinValue;
    private DateTime _lastPurgeUtc = DateTime.MinValue;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await EnsureChannelAsync(stoppingToken);

                    int published = await ProcessOutboxMessagesAsync(stoppingToken);

                    if (published < BatchSize)
                    {
                        await LogStuckMessagesAsync(stoppingToken);
                        await PurgeProcessedMessagesAsync(stoppingToken);
                        await Task.Delay(IdleDelay, stoppingToken);
                    }
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing outbox messages");

                    await DisposeChannelAndConnectionAsync();
                    await Task.Delay(IdleDelay, stoppingToken);
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async Task EnsureChannelAsync(CancellationToken cancellationToken)
    {
        if (_channel is { IsOpen: true })
        {
            return;
        }

        _connection ??= await RabbitMqConnector.ConnectWithRetryAsync(
            settings, logger, "outbox publisher", cancellationToken);

        _channel = await _connection.CreateChannelAsync(
            new CreateChannelOptions(publisherConfirmationsEnabled: true, publisherConfirmationTrackingEnabled: true),
            cancellationToken);
    }

    private async Task<int> ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        List<Message> messages = await context.Messages
            .FromSql(
                $"""
                 SELECT * FROM "Messages"
                 WHERE "ProcessedAt" IS NULL AND "RetryCount" < {MaxPublishAttempts}
                 ORDER BY "CreatedAt"
                 LIMIT {BatchSize}
                 FOR UPDATE SKIP LOCKED
                 """)
            .ToListAsync(cancellationToken);

        if (messages.Count == 0)
        {
            await transaction.CommitAsync(cancellationToken);
            return 0;
        }

        logger.LogInformation("Processing {Count} outbox messages", messages.Count);

        foreach (Message message in messages)
        {
            try
            {
                await PublishAsync(message, cancellationToken);

                message.ProcessedAt = DateTime.UtcNow;
                message.Error = null;

                logger.LogInformation(
                    "Event {EventType} published to queue {QueueName} (MessageId: {MessageId})",
                    message.Type, message.QueueName, message.Id);
            }
            catch (Exception ex)
            {
                message.RetryCount++;
                message.Error = ex.Message;

                logger.LogError(ex, "Failed to publish message {MessageId} to queue {QueueName}",
                    message.Id, message.QueueName);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return messages.Count;
    }

    private async Task PublishAsync(Message message, CancellationToken cancellationToken)
    {
        var properties = new BasicProperties
        {
            Persistent = true,
            MessageId = message.Id,
            ContentType = "application/json"
        };

        await _channel!.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: message.QueueName,
            mandatory: true,
            basicProperties: properties,
            body: Encoding.UTF8.GetBytes(message.Payload),
            cancellationToken: cancellationToken);
    }

    private async Task PurgeProcessedMessagesAsync(CancellationToken cancellationToken)
    {
        if (DateTime.UtcNow - _lastPurgeUtc < PurgeInterval)
        {
            return;
        }

        _lastPurgeUtc = DateTime.UtcNow;

        using IServiceScope scope = serviceProvider.CreateScope();
        ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        DateTime cutoff = DateTime.UtcNow - ProcessedMessageRetention;

        int purged = await context.Messages
            .Where(m => m.ProcessedAt != null && m.ProcessedAt < cutoff)
            .ExecuteDeleteAsync(cancellationToken);

        if (purged > 0)
        {
            logger.LogInformation("Purged {Count} processed outbox messages older than {Days} days",
                purged, ProcessedMessageRetention.TotalDays);
        }
    }

    private async Task LogStuckMessagesAsync(CancellationToken cancellationToken)
    {
        if (DateTime.UtcNow - _lastStuckMessageLogUtc < StuckMessageLogInterval)
        {
            return;
        }

        _lastStuckMessageLogUtc = DateTime.UtcNow;

        using IServiceScope scope = serviceProvider.CreateScope();
        ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        int stuckCount = await context.Messages
            .CountAsync(m => m.ProcessedAt == null && m.RetryCount >= MaxPublishAttempts, cancellationToken);

        if (stuckCount > 0)
        {
            logger.LogError(
                "{Count} outbox messages exhausted {MaxAttempts} publish attempts and need attention",
                stuckCount, MaxPublishAttempts);
        }
    }

    private async Task DisposeChannelAndConnectionAsync()
    {
        try
        {
            if (_channel != null)
            {
                await _channel.DisposeAsync();
            }

            if (_connection != null)
            {
                await _connection.DisposeAsync();
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error discarding the outbox publisher connection");
        }
        finally
        {
            _channel = null;
            _connection = null;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);
        await DisposeChannelAndConnectionAsync();

        logger.LogInformation("Outbox publisher stopped");
    }
}
