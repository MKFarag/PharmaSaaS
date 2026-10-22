namespace PharmaSaaS.Application.Features.Pharmacies.Update;

public record UpdatePharmacyCommand(string Id, PharmacyRequest Request) : IRequest<Result>;
