using System.Threading.Channels;
using DroneBuilder.Application.Common.Abstractions;

namespace DroneBuilder.Infrastructure.Imports;

public class ImportQueue : IImportQueue
{
    private readonly Channel<Guid> _channel = Channel.CreateUnbounded<Guid>();

    public ChannelReader<Guid> Reader => _channel.Reader;

    public ValueTask EnqueueAsync(Guid importRunId, CancellationToken cancellationToken = default)
        => _channel.Writer.WriteAsync(importRunId, cancellationToken);
}
