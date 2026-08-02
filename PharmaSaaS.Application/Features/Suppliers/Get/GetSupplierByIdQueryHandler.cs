namespace PharmaSaaS.Application.Features.Suppliers.Get;

public sealed class GetSupplierByIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetSupplierByIdQuery, Result<SupplierResponse>>
{
    public async Task<Result<SupplierResponse>> Handle(GetSupplierByIdQuery request, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.Suppliers.GetAsync([request.Id], cancellationToken) is not { } supplier)
            return Result.Failure<SupplierResponse>(SupplierErrors.NotFound);

        return Result.Success(supplier.Adapt<SupplierResponse>());
    }
}
