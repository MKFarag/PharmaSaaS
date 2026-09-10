namespace PharmaSaaS.Application.Features.DosageForms.Update;

public sealed class UpdateDosageFormCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService, ICurrentTenantGetter currentTenantGetter) : IRequestHandler<UpdateDosageFormCommand, Result>
{
    public async Task<Result> Handle(UpdateDosageFormCommand command, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.DosageForms.GetAsync([command.Id], cancellationToken) is not { } dosageForm)
            return Result.Failure(DosageFormErrors.NotFound);

        if (await unitOfWork.DosageForms.AnyAsync(d => d.Name == command.Request.Name && d.Id != command.Id, cancellationToken))
            return Result.Failure(DosageFormErrors.DuplicatedName);

        dosageForm.Update(command.Request.Name);

        await cacheService.RemoveByTagAsync(Cache.Tags.DosageForms(currentTenantGetter.GetTenantId()!), cancellationToken);

        await unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
