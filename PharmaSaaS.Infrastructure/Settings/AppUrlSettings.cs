namespace PharmaSaaS.Infrastructure.Settings;

public class AppUrlSettings
{
    public const string SectionName = "AppUrlSettings";
    public string ClientUrl { get; set; } = string.Empty;
    public string ApiUrl { get; set; } = string.Empty;
}
