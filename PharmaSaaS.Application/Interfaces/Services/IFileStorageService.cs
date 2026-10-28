namespace PharmaSaaS.Application.Interfaces.Services;

public interface IFileStorageService
{
    Task<string> SaveAsync(Stream stream, string fileName, CancellationToken cancellationToken = default);
    Task RemoveAsync(string relativePath, CancellationToken cancellationToken = default);
    string GetRelativePath(string fileName);
}
