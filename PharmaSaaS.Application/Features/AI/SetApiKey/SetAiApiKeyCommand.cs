namespace PharmaSaaS.Application.Features.AI.SetApiKey;

public record SetAiApiKeyCommand(SetAiApiKeyRequest Request) : IRequest<Result>;
