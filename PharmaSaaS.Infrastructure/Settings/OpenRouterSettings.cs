namespace PharmaSaaS.Infrastructure.Settings;

public class OpenRouterSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "google/gemini-2.0-flash-lite:free";
    public int MonthlyLimitPerPharmacy { get; set; } = 30;
}
