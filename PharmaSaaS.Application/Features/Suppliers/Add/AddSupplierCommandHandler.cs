using PharmaSaaS.Application.Interfaces.Infrastructure;

namespace PharmaSaaS.Application.Features.Suppliers.Add;

public sealed class AddSupplierCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService, ICurrentTenantGetter tenantGetter)
    : IRequestHandler<AddSupplierCommand, Result<SupplierResponse>>
{
    public async Task<Result<SupplierResponse>> Handle(AddSupplierCommand command, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.Suppliers.AnyAsync(s => s.Name == command.Request.Name, cancellationToken))
            return Result.Failure<SupplierResponse>(SupplierErrors.DuplicateName);

        var supplier = Supplier.Create(command.Request.Name, command.Request.ContactInfo);

        await unitOfWork.Suppliers.AddAsync(supplier, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        await cacheService.RemoveByTagAsync(Cache.Tags.Suppliers(tenantGetter.GetTenantId()!), cancellationToken);

        return Result.Success(supplier.Adapt<SupplierResponse>());
    }
}
