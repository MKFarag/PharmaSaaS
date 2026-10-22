namespace PharmaSaaS.Application.Features.Pharmacies.Get;

public record GetPharmacyQuery(string Id) : IRequest<Result<PharmacyResponse>>;