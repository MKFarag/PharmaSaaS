namespace PharmaSaaS.Application.Features.Pharmacies.Update;

public record UpdatePharmacyCommand(int Id, PharmacyRequest Request) : IRequest<Result>;
