namespace PharmaSaaS.Application.Interfaces.Services;

public interface IAIService
{
    Task<Result<AIResponse>> ParsePrescriptionImageAsync(string pharmacyId, byte[] imageBytes, string contentType, CancellationToken cancellationToken = default);
    Task<Result<AIResponse>> SuggestMedicationsAsync(string pharmacyId, string description, CancellationToken cancellationToken = default);
}
