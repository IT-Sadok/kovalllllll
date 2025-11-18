using Microsoft.AspNetCore.Http;

namespace DroneBuilder.Application.Abstractions;

public interface IAzureStorageService
{
    Task<string> UploadFileAsync(IFormFile file);
    Task DeleteFileAsync(string blobUrl);
}