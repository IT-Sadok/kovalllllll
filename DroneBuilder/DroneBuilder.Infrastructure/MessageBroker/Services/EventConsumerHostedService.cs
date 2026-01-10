using System.Text;
using System.Text.Json;
using DroneBuilder.Application.Abstractions;
using DroneBuilder.Application.Models.NotificationModels;
using DroneBuilder.Domain.Events;
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

            logger.LogInformation("Event received from queue '{Queue}': {Json}", queueName, json);

            using var scope = serviceProvider.CreateScope();

            switch (queueName)
            {
                case "user-signed-up-queue":
                    await HandleUserSignedUpAsync(json, scope, cancellationToken);
                    break;

                default:
                    logger.LogWarning("Unknown queue: {Queue}", queueName);
                    break;
            }

            await _channel!.BasicAckAsync(eventArgs.DeliveryTag, false, cancellationToken);
            logger.LogInformation("Event processed from queue '{Queue}'", queueName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing event from queue '{Queue}'", queueName);
            await _channel!.BasicNackAsync(eventArgs.DeliveryTag, false, false, cancellationToken);
        }
    }

    private async Task HandleUserSignedUpAsync(string json, IServiceScope scope, CancellationToken ct)
    {
        var @event = JsonSerializer.Deserialize<UserSignedUpEvent>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (@event == null)
        {
            logger.LogWarning("Invalid UserSignedUpEvent");
            return;
        }

        logger.LogInformation("UserSignedUpEvent: UserId={UserId}, Email={Email}", @event.UserId, @event.Email);

        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        var notification = new RegistrationNotificationModel(@event.UserId.ToString(), @event.Email);
        await notificationService.SendNotificationAsync(notification);

        logger.LogInformation("Notification sent for UserSignedUpEvent");
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