namespace PharmaSaaS.Application.Features.AI.SuggestMedications;

public sealed class SuggestMedicationsCommandHandler(IAIService aiService, ICurrentTenantGetter currentTenantGetter) : IRequestHandler<SuggestMedicationsCommand, Result<AIResponse>>
{
    public async Task<Result<AIResponse>> Handle(SuggestMedicationsCommand command, CancellationToken cancellationToken = default)
        => await aiService.SuggestMedicationsAsync(currentTenantGetter.GetTenantId()!, command.Request.Description, cancellationToken);
}
