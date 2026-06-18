namespace PharmaSaaS.Application.Features.Pharmacies.ToggleStatus;

public record TogglePharmacyStatusCommand(int Id) : IRequest<Result>;
