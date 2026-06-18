namespace PharmaSaaS.Infrastructure.Services;

public class NotificationService
    (IOptions<EmailTemplateOptions> templateOptions,
    IOptions<AppUrlSettings> appSettings,
    IEmailSender emailSender,
    IJobManager jobManager) : INotificationService
{
    private readonly EmailTemplateOptions _templateOptions = templateOptions.Value;
    private readonly string _clientBaseUrl = appSettings.Value.ClientUrl;
    private readonly IEmailSender _emailSender = emailSender;
    private readonly IJobManager _jobManager = jobManager;

    public async Task SendConfirmationLinkAsync(User user, string token, int expiryTimeInHours = 24)
    {
        var link = $"{_clientBaseUrl}/auth/emailConfirmation?userId={user.Id}&token={token}";

        var placeholders = BuildPlaceholders(user.FirstName, new Dictionary<string, string>
        {
            { EmailTemplateOptions.Placeholders.ActionUrl, link },
            { EmailTemplateOptions.Placeholders.ExpiryTime, expiryTimeInHours.ToString() }
        });

        await EnqueueEmailAsync(user.Email!, $"{_templateOptions.TitleName} — Email Confirmation", EmailTemplateOptions.TemplatesNames.EmailConfirmationLink, placeholders);
    }

    public async Task SendResetPasswordAsync(User user, string token, int expiryTimeInHours = 24)
    {
        var link = $"{_clientBaseUrl}/auth/forgetPassword?email={user.Email}&token={token}";

        var placeholders = BuildPlaceholders(user.FirstName, new Dictionary<string, string>
        {
            { EmailTemplateOptions.Placeholders.ActionUrl, link },
            { EmailTemplateOptions.Placeholders.ExpiryTime, expiryTimeInHours.ToString() }
        });

        await EnqueueEmailAsync(user.Email!, $"{_templateOptions.TitleName} — Reset Password", EmailTemplateOptions.TemplatesNames.ResetPassword, placeholders);
    }

    public async Task SendChangeEmailNotificationAsync(User user, string oldEmail, DateTime changeDate)
    {
        var placeholders = BuildPlaceholders(user.FirstName, new Dictionary<string, string>
        {
            { EmailTemplateOptions.Placeholders.NewEmail, user.Email! },
            { EmailTemplateOptions.Placeholders.ChangeDate, changeDate.ToString("f") }
        });

        await EnqueueEmailAsync(oldEmail, $"{_templateOptions.TitleName} — Email Address Changed", EmailTemplateOptions.TemplatesNames.ChangeEmailNotification, placeholders);
    }

    private async Task EnqueueEmailAsync(string email, string subject, string templateName, Dictionary<string, string> placeholders)
    {
        var body = await TemplateRenderer.RenderAsync(templateName, placeholders);
        _jobManager.Enqueue(() => _emailSender.SendEmailAsync(email, subject, body));
    }

    private Dictionary<string, string> BuildPlaceholders(string userName, Dictionary<string, string>? additionalPlaceholders = null)
    {
        var dictionary = new Dictionary<string, string>
        {
            { EmailTemplateOptions.Placeholders.TitleName, _templateOptions.TitleName },
            { EmailTemplateOptions.Placeholders.TeamName, _templateOptions.TeamName },
            { EmailTemplateOptions.Placeholders.Address, _templateOptions.Address },
            { EmailTemplateOptions.Placeholders.City, _templateOptions.City },
            { EmailTemplateOptions.Placeholders.Country, _templateOptions.Country },
            { EmailTemplateOptions.Placeholders.SupportEmail, _templateOptions.SupportEmail },
            { EmailTemplateOptions.Placeholders.UserName, userName }
        };

        if (additionalPlaceholders is not null)
            foreach (var item in additionalPlaceholders)
                dictionary[item.Key] = item.Value;

        return dictionary;
    }
}
