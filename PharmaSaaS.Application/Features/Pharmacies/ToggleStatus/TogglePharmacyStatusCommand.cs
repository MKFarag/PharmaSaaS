namespace PharmaSaaS.Application.Features.Pharmacies.ToggleStatus;

public record TogglePharmacyStatusCommand(string Id) : IRequest<Result>;
