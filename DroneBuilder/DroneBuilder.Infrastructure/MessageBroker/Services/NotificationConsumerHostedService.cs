using System.Text;
using System.Text.Json;
using DroneBuilder.Application.Models.NotificationModels;
using DroneBuilder.Infrastructure.MessageBroker.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace DroneBuilder.Infrastructure.MessageBroker.Services;

public class NotificationConsumerHostedService(
    RabbitMqConfiguration settings,
    ILogger<NotificationConsumerHostedService> logger) : BackgroundService
{
    private IConnection? _connection;
    private IChannel? _channel;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await InitializeRabbitMqAsync(stoppingToken);
            await StartConsumingAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error starting notification consumer");
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

        logger.LogInformation("RabbitMQ Consumer connected to queue: {Queue}",
            settings.NotificationQueueName);
    }

    private async Task StartConsumingAsync(CancellationToken cancellationToken)
    {
        if (_channel == null)
            throw new InvalidOperationException("RabbitMQ channel is not initialized");

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (sender, eventArgs) =>
        {
            try
            {
                var body = eventArgs.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);
                var notification = JsonSerializer.Deserialize<NotificationMessageModel>(json);

                if (notification != null)
                {
                    logger.LogInformation(
                        "Notification received from RabbitMQ. " +
                        "UserId: {UserId}, Type: {Type}, Title: {Title}, Message: {Message}, CreatedAt: {CreatedAt}, Metadata: {@Metadata}",
                        notification.UserId,
                        notification.Type,
                        notification.Title,
                        notification.Message,
                        notification.CreatedAt,
                        notification.Metadata
                    );

                    await _channel.BasicAckAsync(eventArgs.DeliveryTag, false, cancellationToken);

                    logger.LogInformation(
                        "Notification processed and acknowledged. Title: {Title}, UserId: {UserId}",
                        notification.Title,
                        notification.UserId
                    );
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing notification message");

                await _channel.BasicNackAsync(eventArgs.DeliveryTag, false, false, cancellationToken);
            }
        };

        await _channel.BasicConsumeAsync(
            queue: settings.NotificationQueueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: cancellationToken
        );

        logger.LogInformation("Started consuming messages from queue: {Queue}",
            settings.NotificationQueueName);

        await Task.Delay(Timeout.Infinite, cancellationToken);
    }

    public override async void Dispose()
    {
        try
        {
            if (_channel != null) await _channel.CloseAsync();
            if (_connection != null) await _connection.CloseAsync();
            logger.LogInformation("NotificationConsumerHostedService disposed");
            base.Dispose();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error disposing NotificationConsumerHostedService");
        }
    }
}