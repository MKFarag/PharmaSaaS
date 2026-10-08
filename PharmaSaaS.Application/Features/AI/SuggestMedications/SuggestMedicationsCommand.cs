namespace PharmaSaaS.Application.Features.AI.SuggestMedications;

public record SuggestMedicationsCommand(SuggestMedicationsRequest Request) : IRequest<Result<AIResponse>>;
