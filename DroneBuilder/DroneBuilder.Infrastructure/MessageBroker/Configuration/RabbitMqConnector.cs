using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace DroneBuilder.Infrastructure.MessageBroker.Configuration;

internal static class RabbitMqConnector
{
    private static readonly TimeSpan InitialRetryDelay = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan MaxRetryDelay = TimeSpan.FromSeconds(60);

    public static async Task<IConnection> ConnectWithRetryAsync(
        RabbitMqConfiguration settings,
        ILogger logger,
        string purpose,
        CancellationToken cancellationToken)
    {
        ConnectionFactory factory = CreateFactory(settings);
        TimeSpan delay = InitialRetryDelay;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                IConnection connection = await factory.CreateConnectionAsync(cancellationToken);
                logger.LogInformation("Connected to RabbitMQ for {Purpose}.", purpose);
                return connection;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Could not connect to RabbitMQ for {Purpose}. Retrying in {Delay}.", purpose, delay);

                await Task.Delay(delay, cancellationToken);
                delay = TimeSpan.FromTicks(Math.Min(delay.Ticks * 2, MaxRetryDelay.Ticks));
            }
        }
    }

    private static ConnectionFactory CreateFactory(RabbitMqConfiguration settings)
    {
        if (!string.IsNullOrEmpty(settings.ConnectionString))
        {
            var uri = new Uri(settings.ConnectionString);

            return new ConnectionFactory
            {
                Uri = uri,
                AutomaticRecoveryEnabled = true,
                Ssl = new SslOption
                {
                    Enabled = true,
                    ServerName = uri.Host
                }
            };
        }

        return new ConnectionFactory
        {
            HostName = settings.HostName,
            Port = settings.Port,
            UserName = settings.UserName,
            Password = settings.Password,
            VirtualHost = settings.VirtualHost,
            AutomaticRecoveryEnabled = true
        };
    }
}
