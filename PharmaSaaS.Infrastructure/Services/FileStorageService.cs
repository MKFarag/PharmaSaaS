using Microsoft.AspNetCore.Hosting;
using PharmaSaaS.Application.Interfaces.Services;

namespace PharmaSaaS.Infrastructure.Services;

public sealed class FileStorageService : IFileStorageService
{
    private const string _subFolder = "images/payments";
    private readonly string _imagesPath;
    private readonly string _rootPath;

    public FileStorageService(IWebHostEnvironment webHostEnvironment)
    {
        _rootPath = webHostEnvironment.WebRootPath ?? Path.Combine(webHostEnvironment.ContentRootPath, "wwwroot");
        _imagesPath = Path.Combine(_rootPath, "images", "payments");

        if (!Directory.Exists(_imagesPath))
            Directory.CreateDirectory(_imagesPath);
    }

    public async Task<string> SaveAsync(Stream stream, string fileName, CancellationToken cancellationToken = default)
    {
        var filePath = Path.Combine(_imagesPath, fileName);

        await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        await stream.CopyToAsync(fileStream, cancellationToken);

        return GetRelativePath(fileName);
    }

    public Task RemoveAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        var sanitized = relativePath.TrimStart('/', '\\').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.Combine(_rootPath, sanitized);

        if (File.Exists(fullPath))
            File.Delete(fullPath);

        return Task.CompletedTask;
    }

    public string GetRelativePath(string fileName)
        => $"/{_subFolder}/{fileName}";
}
