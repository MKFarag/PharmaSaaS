namespace PharmaSaaS.Application.Features.Pharmacies.GetAll;

public sealed class GetAllPharmaciesQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetAllPharmaciesQuery, IEnumerable<PharmacyResponse>>
{
    public async Task<IEnumerable<PharmacyResponse>> Handle(GetAllPharmaciesQuery request, CancellationToken cancellationToken = default)
        => await unitOfWork.Pharmacies.GetAllAsync<PharmacyResponse>(cancellationToken);
}
