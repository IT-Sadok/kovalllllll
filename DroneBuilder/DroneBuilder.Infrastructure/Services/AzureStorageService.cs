using Azure.Storage.Blobs;
using DroneBuilder.Infrastructure.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using DroneBuilder.Application.Abstractions;

namespace DroneBuilder.Infrastructure.Services;

public class AzureStorageService(IOptions<AzureStorageConfig> config) : IAzureStorageService
{
    private readonly BlobServiceClient _blobServiceClient = new(config.Value.ConnectionString);
    private readonly string _containerName = config.Value.ContainerName;

    public async Task<string> UploadFileAsync(IFormFile file)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        await containerClient.CreateIfNotExistsAsync();
        var blobClient = containerClient.GetBlobClient(Guid.NewGuid() + Path.GetExtension(file.FileName));

        await using (var stream = file.OpenReadStream())
        {
            await blobClient.UploadAsync(stream, true);
        }

        return blobClient.Uri.ToString();
    }

    public async Task DeleteFileAsync(string blobUrl)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        var blobName = new Uri(blobUrl).Segments.Last();
        var blobClient = containerClient.GetBlobClient(blobName);
        await blobClient.DeleteIfExistsAsync();
    }
}