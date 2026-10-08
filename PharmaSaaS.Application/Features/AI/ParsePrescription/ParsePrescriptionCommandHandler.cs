namespace PharmaSaaS.Application.Features.AI.ParsePrescription;

public sealed class ParsePrescriptionCommandHandler(IAIService aiService, ICurrentTenantGetter currentTenantGetter) : IRequestHandler<ParsePrescriptionCommand, Result<AIResponse>>
{
    public async Task<Result<AIResponse>> Handle(ParsePrescriptionCommand command, CancellationToken cancellationToken = default)
        => await aiService.ParsePrescriptionImageAsync(currentTenantGetter.GetTenantId()!, command.ImageBytes, command.ContentType, cancellationToken);
}
