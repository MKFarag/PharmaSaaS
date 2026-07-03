namespace PharmaSaaS.Application.Features.DosageForms.Delete;

public class DeleteDosageFormCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService, ICurrentTenantGetter currentTenantGetter) : IRequestHandler<DeleteDosageFormCommand, Result>
{
    public async Task<Result> Handle(DeleteDosageFormCommand command, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.DosageForms.GetAsync([command.Id], cancellationToken) is not { } dosageForm)
            return Result.Failure(DosageFormErrors.NotFound);

        if (await unitOfWork.Products.AnyAsync(p => p.DosageFormId == command.Id, cancellationToken))
            return Result.Failure(DosageFormErrors.IsUsed);

        unitOfWork.DosageForms.Delete(dosageForm);

        await unitOfWork.CompleteAsync(cancellationToken);

        await cacheService.RemoveByTagAsync(Cache.Tags.DosageForms(currentTenantGetter.GetTenantId()!), cancellationToken);

        return Result.Success();
    }
}
