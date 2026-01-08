using RabbitMQ.Client;

namespace DroneBuilder.Infrastructure.MessageBroker.Configuration;

public class RabbitMqConfiguration
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";

    public string NotificationQueueName { get; set; } = "notifications";
    public string NotificationExchange { get; set; } = "notifications.exchange";
    public string NotificationRoutingKey { get; set; } = "notification.sent";

    public async Task<IConnection> CreateConnectionAsync(CancellationToken cancellationToken = default)
    {
        var factory = new ConnectionFactory
        {
            HostName = HostName,
            Port = Port,
            UserName = UserName,
            Password = Password,
            VirtualHost = VirtualHost,

            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
        };

        return await factory.CreateConnectionAsync(cancellationToken);
    }
}