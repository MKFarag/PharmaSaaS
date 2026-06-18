namespace PharmaSaaS.Application.Features.Auth.Register;

public record RegisterCommand(RegisterRequest Request) : IRequest<Result>;
