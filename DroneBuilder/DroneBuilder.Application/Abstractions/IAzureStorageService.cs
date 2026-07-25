namespace DroneBuilder.Application.Abstractions;

public interface IAzureStorageService
{
    Task<(bool success, string url)> UploadFileAsync(FileUpload file, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(string blobUrl, CancellationToken cancellationToken = default);
}
