namespace PharmaSaaS.Infrastructure.Settings;

public class MailSettings
{
    [Required, EmailAddress]
    public string Mail { get; init; } = string.Empty;

    [Required]
    public string DisplayName { get; init; } = string.Empty;

    [Required]
    public string Password { get; init; } = string.Empty;

    [Required]
    public string Host { get; init; } = string.Empty;

    [Range(100, 999)]
    public int Port { get; init; }
}
