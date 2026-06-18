namespace PharmaSaaS.Application.Features.Pharmacies.GetAll;

public record GetAllPharmaciesQuery : IRequest<IEnumerable<PharmacyResponse>>;
