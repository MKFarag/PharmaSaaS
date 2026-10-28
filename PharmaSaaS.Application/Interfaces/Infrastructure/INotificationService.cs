namespace PharmaSaaS.Application.Interfaces.Infrastructure;

public interface INotificationService
{
    Task SendConfirmationLinkAsync(User user, string token, int expiryTimeInHours = 24);
    Task SendResetPasswordAsync(User user, string token, int expiryTimeInHours = 24);
    Task SendChangeEmailNotificationAsync(User user, string oldEmail, DateTime changeDate);
    Task SendNewSubscriptionRequestNotificationAsync(string pharmacyName, string ownerName, string phoneNumber, string email, DateTime requestDate);
    Task SendSubscriptionRequestStatusUpdateAsync(string requesterEmail, string pharmacyName, string status, DateOnly? subscriptionEnd = null);
    Task SendExpiredSubscriptionsReportAsync(IReadOnlyList<string> deactivatedPharmacyNames);
    Task SendPharmacySubscriptionExpiredAsync(string ownerEmail, string ownerName, string pharmacyName, DateOnly subscriptionEnd);
    Task SendSubscriptionPaymentNotificationAsync(string pharmacyName, string ownerName, string phoneNumber, string email, string imageUrl, DateTime submittedAt);
}
