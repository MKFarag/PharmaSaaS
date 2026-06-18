namespace PharmaSaaS.Application.Interfaces.Infrastructure;

public interface INotificationService
{
    Task SendConfirmationLinkAsync(User user, string token, int expiryTimeInHours = 24);
    Task SendResetPasswordAsync(User user, string token, int expiryTimeInHours = 24);
    Task SendChangeEmailNotificationAsync(User user, string oldEmail, DateTime changeDate);
}
