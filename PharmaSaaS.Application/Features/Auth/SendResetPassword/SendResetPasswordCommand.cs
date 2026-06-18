namespace PharmaSaaS.Application.Features.Auth.SendResetPassword;

public record SendResetPasswordCommand(string Email) : IRequest<Result>;
