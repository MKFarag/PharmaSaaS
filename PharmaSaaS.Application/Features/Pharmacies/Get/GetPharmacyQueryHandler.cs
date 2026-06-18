namespace PharmaSaaS.Application.Features.Pharmacies.Get;

public class GetPharmacyQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetPharmacyQuery, Result<PharmacyResponse>>
{
    public async Task<Result<PharmacyResponse>> Handle(GetPharmacyQuery request, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.Pharmacies.GetAsync(request.Id, cancellationToken) is not { } pharmacy)
            return Result.Failure<PharmacyResponse>(PharmacyErrors.NotFound);

        return Result.Success(pharmacy.Adapt<PharmacyResponse>());
    }
}
