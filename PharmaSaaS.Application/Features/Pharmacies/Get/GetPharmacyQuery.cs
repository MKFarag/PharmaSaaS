namespace PharmaSaaS.Application.Features.Pharmacies.Get;

public record GetPharmacyQuery(int Id) : IRequest<Result<PharmacyResponse>>;