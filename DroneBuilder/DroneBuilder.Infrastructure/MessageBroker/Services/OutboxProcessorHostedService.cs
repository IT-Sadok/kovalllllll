using System.Text;
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

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await InitializeRabbitMqAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing outbox messages");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
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

        logger.LogInformation("RabbitMQ Producer initialized");
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
                var body = Encoding.UTF8.GetBytes(message.Payload);

                await _channel.BasicPublishAsync(
                    exchange: "",
                    routingKey: message.QueueName,
                    mandatory: true,
                    basicProperties: new BasicProperties
                    {
                        Persistent = true,
                        MessageId = message.Id,
                        ContentType = "application/json"
                    },
                    body: body,
                    cancellationToken: cancellationToken
                );

                message.ProcessedAt = DateTime.UtcNow;
                logger.LogInformation(
                    "Event {EventType} published to queue '{QueueName}' (MessageId: {MessageId})",
                    message.Type,
                    message.QueueName,
                    message.Id
                );
            }
            catch (Exception ex)
            {
                message.RetryCount++;
                message.Error = ex.Message;
                logger.LogError(ex, "Failed to publish message {MessageId} to queue '{QueueName}'",
                    message.Id, message.QueueName);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    public override async void Dispose()
    {
        try
        {
            if (_channel != null) await _channel.CloseAsync();
            if (_connection != null) await _connection.CloseAsync();
            logger.LogInformation("OutboxProcessorHostedService disposed");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error disposing OutboxProcessorHostedService");
        }

        base.Dispose();
    }
}