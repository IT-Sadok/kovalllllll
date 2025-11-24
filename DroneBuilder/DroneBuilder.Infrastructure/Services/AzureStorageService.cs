using Azure.Storage.Blobs;
using DroneBuilder.Infrastructure.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using DroneBuilder.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace DroneBuilder.Infrastructure.Services;

public class AzureStorageService(IOptions<AzureStorageConfig> config, ILogger<AzureStorageService> logger)
    : IAzureStorageService
{
    private readonly BlobServiceClient _blobServiceClient = new(config.Value.ConnectionString);
    private readonly string _containerName = config.Value.ContainerName;

    public async Task<(bool success, string url)> UploadFileAsync(IFormFile file,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Uploading file {FileName} to Azure Blob Storage.", file.FileName);
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            await containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

            var blobClient = containerClient.GetBlobClient(file.FileName);

            await using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, overwrite: true, cancellationToken);
            logger.LogInformation(
                "File {FileName} uploaded successfully to container {BlobContainerName} with URL {Url}.",
                file.FileName, blobClient.BlobContainerName, blobClient.Uri);

            return (true, blobClient.Uri.ToString());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error uploading file {FileName} to Azure Blob Storage.", file.FileName);
            return (false, string.Empty);
        }
    }

    public async Task DeleteFileAsync(string blobUrl, CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        var blobName = new Uri(blobUrl).Segments.Last();
        var blobClient = containerClient.GetBlobClient(blobName);
        await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }
}