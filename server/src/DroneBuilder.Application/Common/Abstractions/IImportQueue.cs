namespace DroneBuilder.Application.Common.Abstractions;

public interface IImportQueue
{
    ValueTask EnqueueAsync(Guid importRunId, CancellationToken cancellationToken = default);
}
