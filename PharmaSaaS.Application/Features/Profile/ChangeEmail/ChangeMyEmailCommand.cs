namespace PharmaSaaS.Application.Features.Profile.ChangeEmail;

public record ChangeMyEmailCommand(string UserId, string NewEmail) : IRequest<Result>;
