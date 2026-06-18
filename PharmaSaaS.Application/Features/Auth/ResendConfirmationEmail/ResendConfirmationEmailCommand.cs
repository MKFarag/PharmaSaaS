namespace PharmaSaaS.Application.Features.Auth.ResendConfirmationEmail;

public record ResendConfirmationEmailCommand(string Email) : IRequest<Result>;
