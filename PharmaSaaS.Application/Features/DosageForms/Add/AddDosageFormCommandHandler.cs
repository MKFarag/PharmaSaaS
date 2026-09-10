namespace PharmaSaaS.Application.Features.DosageForms.Add;

public sealed class AddDosageFormCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService, ICurrentTenantGetter currentTenantGetter) : IRequestHandler<AddDosageFormCommand, Result<DosageFormResponse>>
{
    public async Task<Result<DosageFormResponse>> Handle(AddDosageFormCommand command, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.DosageForms.AnyAsync(d => d.Name == command.Request.Name, cancellationToken))
            return Result.Failure<DosageFormResponse>(DosageFormErrors.DuplicatedName);

        var dosageForm = DosageForm.Create(command.Request.Name);

        await unitOfWork.DosageForms.AddAsync(dosageForm, cancellationToken);

        await cacheService.RemoveByTagAsync(Cache.Tags.DosageForms(currentTenantGetter.GetTenantId()!), cancellationToken);

        await unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success(dosageForm.Adapt<DosageFormResponse>());
    }
}
