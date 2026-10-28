namespace PharmaSaaS.Application.Contracts.Common;

public record FileData(
    string FileName,
    string ContentType,
    long Length,
    Stream Stream
) : IDisposable
{
    private bool _disposed;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
                Stream?.Dispose();

            _disposed = true;
        }
    }
}
