using System.Text;

namespace DroneBuilder.Infrastructure.MessageBroker.Configuration;

/// <summary>
/// Redelivery count carried on the message itself. AMQP header values arrive with whatever type the
/// publisher used, and strings come back as raw bytes, so reading one back is deliberately lenient.
/// </summary>
internal static class MessageRetryHeader
{
    public const string Name = "x-retry-count";

    public static int Read(IDictionary<string, object?>? headers)
    {
        if (headers is null || !headers.TryGetValue(Name, out object? raw) || raw is null)
        {
            return 0;
        }

        int value = raw switch
        {
            int intValue => intValue,
            long longValue => (int)longValue,
            short shortValue => shortValue,
            byte[] bytes => int.TryParse(Encoding.UTF8.GetString(bytes), out int parsed) ? parsed : 0,
            string text => int.TryParse(text, out int parsed) ? parsed : 0,
            _ => 0
        };

        // A negative or nonsensical value must not be able to grant unlimited redeliveries.
        return value < 0 ? 0 : value;
    }

    public static string DeadLetterQueueName(string queueName) => $"{queueName}.dead-letter";
}
