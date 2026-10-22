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

    public async Task SendNewSubscriptionRequestNotificationAsync(string pharmacyName, string ownerName, string phoneNumber, string email, DateTime requestDate)
    {
        if (string.IsNullOrWhiteSpace(_templateOptions.PlatformAdminEmail))
            return;

        var placeholders = BuildPlaceholders("Mohamed", new Dictionary<string, string>
        {
            { EmailTemplateOptions.Placeholders.PharmacyName, pharmacyName },
            { EmailTemplateOptions.Placeholders.OwnerName, ownerName },
            { EmailTemplateOptions.Placeholders.PhoneNumber, phoneNumber },
            { EmailTemplateOptions.Placeholders.Email, email },
            { EmailTemplateOptions.Placeholders.RequestDate, requestDate.ToString("f") }
        });

        await EnqueueEmailAsync(_templateOptions.PlatformAdminEmail, $"{_templateOptions.TitleName} — New Subscription Request", EmailTemplateOptions.TemplatesNames.NewSubscriptionRequestNotification, placeholders);
    }

    public async Task SendSubscriptionRequestStatusUpdateAsync(string requesterEmail, string pharmacyName, string status, DateOnly? subscriptionEnd = null)
    {
        var statusClass = status.ToLowerInvariant();
        var statusLabel = status;
        var statusMessage = status switch
        {
            "Approved" => "Congratulations! Your pharmacy has been approved. You will receive further instructions to set up your account.",
            "Rejected" => "Unfortunately, your subscription request has been rejected. If you have any questions, please contact our support team.",
            "Contacted" => "Our team has been notified about your request. We will reach out to you shortly to discuss the next steps.",
            _ => "Your subscription request status has been updated."
        };

        var placeholders = BuildPlaceholders(pharmacyName, new Dictionary<string, string>
        {
            { EmailTemplateOptions.Placeholders.PharmacyName, pharmacyName },
            { EmailTemplateOptions.Placeholders.RequestStatus, statusLabel },
            { EmailTemplateOptions.Placeholders.StatusClass, statusClass },
            { EmailTemplateOptions.Placeholders.StatusMessage, statusMessage },
            { EmailTemplateOptions.Placeholders.UpdateDate, DateTime.UtcNow.ToString("f") }
        });

        if (subscriptionEnd.HasValue)
        {
            var subscriptionRow =
                $@"<div class=""info-row"">
                    <span class=""info-key"">Subscription end</span>
                    <span class=""info-val"">{subscriptionEnd.Value:yyyy-MM-dd}</span>
                </div>";

            placeholders[EmailTemplateOptions.Placeholders.SubscriptionEnd] = subscriptionRow;
        }

        await EnqueueEmailAsync(requesterEmail, $"{_templateOptions.TitleName} — Subscription Request {statusLabel}", EmailTemplateOptions.TemplatesNames.SubscriptionRequestStatusUpdate, placeholders);
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
