namespace PharmaSaaS.Domain.Constants;

public static class FileSettings
{
    public const int MaxFileSizeInMb = 8;
    public const long MaxFileSizeInBytes = MaxFileSizeInMb * 1024L * 1024L;
    public static readonly string[] BlockedSignatures = ["4D-5A", "2F-2A", "D0-CF"];
    public static readonly string[] AllowedImagesExtensions = [".jpg", ".jpeg", ".png"];
}
