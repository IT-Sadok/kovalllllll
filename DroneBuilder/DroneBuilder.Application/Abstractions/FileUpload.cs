namespace DroneBuilder.Application.Abstractions;

public sealed record FileUpload(
    Stream Content,
    string FileName,
    string ContentType,
    long Length);
