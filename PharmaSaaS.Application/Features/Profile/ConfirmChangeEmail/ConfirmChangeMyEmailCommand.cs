namespace PharmaSaaS.Application.Features.Profile.ConfirmChangeEmail;

public record ConfirmChangeMyEmailCommand(string UserId, string NewEmail, string Token) : IRequest<Result>;
