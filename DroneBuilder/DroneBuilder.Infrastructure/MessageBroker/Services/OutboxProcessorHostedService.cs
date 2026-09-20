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

    private IConnection? _connection;
    private IChannel? _channel;
    private DateTime _lastStuckMessageLogUtc = DateTime.MinValue;

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

                    // A full batch means there is probably more waiting, so drain before idling.
                    if (published < BatchSize)
                    {
                        await LogStuckMessagesAsync(stoppingToken);
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

                    // The connection may be the reason; drop it so the next pass reconnects.
                    await DisposeChannelAndConnectionAsync();
                    await Task.Delay(IdleDelay, stoppingToken);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown.
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

        // Publisher confirmations make BasicPublishAsync wait for the broker to take responsibility
        // for the message. Without them a message could be marked as processed and still be lost.
        _channel = await _connection.CreateChannelAsync(
            new CreateChannelOptions(publisherConfirmationsEnabled: true, publisherConfirmationTrackingEnabled: true),
            cancellationToken);
    }

    private async Task<int> ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        // SKIP LOCKED keeps two instances of the API from publishing the same message twice.
        // Nothing may be composed onto this query (no Where, no OrderBy): EF would wrap it in a
        // subquery and FOR UPDATE is not valid there.
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

        // mandatory plus confirmations means an unroutable message throws instead of vanishing.
        await _channel!.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: message.QueueName,
            mandatory: true,
            basicProperties: properties,
            body: Encoding.UTF8.GetBytes(message.Payload),
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Messages that exhausted their attempts are skipped by the query forever, so they are reported
    /// periodically rather than sitting in the table unnoticed.
    /// </summary>
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
