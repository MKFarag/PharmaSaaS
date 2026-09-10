namespace PharmaSaaS.Application.Features.DosageForms.GetAll;

public sealed class GetAllDosageFormsQueryHandler(IUnitOfWork unitOfWork, ICacheService cacheService, ICurrentTenantGetter currentTenantGetter) : IRequestHandler<GetAllDosageFormsQuery, IEnumerable<DosageFormResponse>>
{
    public async Task<IEnumerable<DosageFormResponse>> Handle(GetAllDosageFormsQuery request, CancellationToken cancellationToken = default)
    {
        var tenantId = currentTenantGetter.GetTenantId();

        return await cacheService
            .GetOrCreateAsync
            (
                Cache.Keys.AllDosageForms(tenantId!),
                unitOfWork.DosageForms.GetAllProjectionAsync<DosageFormResponse>,
                Cache.Expirations.VeryLong,
                [Cache.Tags.DosageForms(tenantId!)],
                cancellationToken
            );
    }
}
