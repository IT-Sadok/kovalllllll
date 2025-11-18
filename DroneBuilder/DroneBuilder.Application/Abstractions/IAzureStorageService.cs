using Microsoft.AspNetCore.Http;

namespace DroneBuilder.Application.Abstractions;

public interface IAzureStorageService
{
    Task<(bool success, string url)> UploadFileAsync(IFormFile file, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(string blobUrl, CancellationToken cancellationToken = default);
}