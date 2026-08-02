using PharmaSaaS.Application.Interfaces.Infrastructure;

namespace PharmaSaaS.Application.Features.Suppliers.Update;

public sealed class UpdateSupplierCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService, ICurrentTenantGetter tenantGetter)
    : IRequestHandler<UpdateSupplierCommand, Result>
{
    public async Task<Result> Handle(UpdateSupplierCommand command, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.Suppliers.GetAsync([command.Id], cancellationToken) is not { } supplier)
            return Result.Failure(SupplierErrors.NotFound);

        if (await unitOfWork.Suppliers.AnyAsync(s => s.Name == command.Request.Name && s.Id != command.Id, cancellationToken))
            return Result.Failure(SupplierErrors.DuplicateName);

        supplier.Update(command.Request.Name, command.Request.ContactInfo);

        await unitOfWork.CompleteAsync(cancellationToken);

        await cacheService.RemoveByTagAsync(Cache.Tags.Suppliers(tenantGetter.GetTenantId()!), cancellationToken);

        return Result.Success();
    }
}
