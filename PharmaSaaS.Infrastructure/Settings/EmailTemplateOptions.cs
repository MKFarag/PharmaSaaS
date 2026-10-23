namespace PharmaSaaS.Infrastructure.Settings;

public class EmailTemplateOptions
{
    public string TitleName { get; init; } = string.Empty;
    public string TeamName { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
    public string SupportEmail { get; init; } = string.Empty;
    public string PlatformAdminName { get; init; } = "Mohamed";
    public string PlatformAdminEmail { get; init; } = string.Empty;

    public partial class Placeholders
    {
        public static readonly string TitleName = "{{TitleName}}";
        public static readonly string UserName = "{{UserName}}";
        public static readonly string TeamName = "{{TeamName}}";
        public static readonly string SupportEmail = "{{SupportEmail}}";
        public static readonly string Address = "{{Address}}";
        public static readonly string City = "{{City}}";
        public static readonly string Country = "{{Country}}";
        public static readonly string ActionUrl = "{{ActionUrl}}";
        public static readonly string ChangeDate = "{{ChangeDate}}";
        public static readonly string NewEmail = "{{NewEmail}}";
        public static readonly string ExpiryTime = "{{ExpiryTime}}";
        public static readonly string PharmacyName = "{{PharmacyName}}";
        public static readonly string OwnerName = "{{OwnerName}}";
        public static readonly string PhoneNumber = "{{PhoneNumber}}";
        public static readonly string Email = "{{Email}}";
        public static readonly string RequestDate = "{{RequestDate}}";
        public static readonly string RequestStatus = "{{RequestStatus}}";
        public static readonly string SubscriptionEnd = "{{SubscriptionEnd}}";
        public static readonly string StatusClass = "{{StatusClass}}";
        public static readonly string UpdateDate = "{{UpdateDate}}";
        public static readonly string StatusMessage = "{{StatusMessage}}";
        public static readonly string PharmacyList = "{{PharmacyList}}";
        public static readonly string DeactivationDate = "{{DeactivationDate}}";
        public static readonly string Count = "{{Count}}";
    }

    public partial class TemplatesNames
    {
        public static readonly string EmailConfirmationLink = nameof(EmailConfirmationLink);
        public static readonly string ChangeEmailNotification = nameof(ChangeEmailNotification);
        public static readonly string ResetPassword = nameof(ResetPassword);
        public static readonly string NewSubscriptionRequestNotification = nameof(NewSubscriptionRequestNotification);
        public static readonly string SubscriptionRequestStatusUpdate = nameof(SubscriptionRequestStatusUpdate);
        public static readonly string ExpiredSubscriptionsReport = nameof(ExpiredSubscriptionsReport);
        public static readonly string PharmacySubscriptionExpired = nameof(PharmacySubscriptionExpired);
    }
}
