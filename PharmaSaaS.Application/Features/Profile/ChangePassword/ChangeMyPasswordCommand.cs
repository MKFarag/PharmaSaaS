namespace PharmaSaaS.Application.Features.Profile.ChangePassword;

public record ChangeMyPasswordCommand(string UserId, string CurrentPassword, string NewPassword) : IRequest<Result>;
