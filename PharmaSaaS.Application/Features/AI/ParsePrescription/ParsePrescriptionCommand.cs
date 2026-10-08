namespace PharmaSaaS.Application.Features.AI.ParsePrescription;

public record ParsePrescriptionCommand(byte[] ImageBytes, string ContentType) : IRequest<Result<AIResponse>>;
