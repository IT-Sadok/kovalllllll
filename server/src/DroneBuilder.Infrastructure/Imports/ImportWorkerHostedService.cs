using DroneBuilder.Application.Common.Repositories;
using DroneBuilder.Application.Features.Imports.RaceDayQuads;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DroneBuilder.Infrastructure.Imports;

public class ImportWorkerHostedService(
    IServiceProvider serviceProvider,
    ImportQueue queue,
    ILogger<ImportWorkerHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await FailInterruptedRunsAsync(stoppingToken);

        await foreach (Guid importRunId in queue.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                using IServiceScope scope = serviceProvider.CreateScope();
                IRaceDayQuadsImporter importer = scope.ServiceProvider.GetRequiredService<IRaceDayQuadsImporter>();
                await importer.RunAsync(importRunId, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Import run {ImportRunId} crashed", importRunId);
            }
        }
    }

    private async Task FailInterruptedRunsAsync(CancellationToken cancellationToken)
    {
        try
        {
            using IServiceScope scope = serviceProvider.CreateScope();
            IImportRunRepository runs = scope.ServiceProvider.GetRequiredService<IImportRunRepository>();
            int failed = await runs.FailUnfinishedRunsAsync("Interrupted by an application restart.", DateTime.UtcNow,
                cancellationToken);

            if (failed > 0)
            {
                logger.LogWarning("Marked {Count} unfinished import runs as failed", failed);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Could not mark unfinished import runs as failed");
        }
    }
}
