namespace PharmaSaaS.Application.Features.Pharmacies.Update;

public sealed class UpdatePharmacyCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<UpdatePharmacyCommand, Result>
{
    public async Task<Result> Handle(UpdatePharmacyCommand command, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.Pharmacies.GetAsync(command.Id, cancellationToken) is not { } pharmacy)
            return Result.Failure(PharmacyErrors.NotFound);

        if (await unitOfWork.Pharmacies.NameExistsAsync(pharmacy.TenantId, command.Request.Name, command.Id, cancellationToken))
            return Result.Failure(PharmacyErrors.DuplicatedName);

        pharmacy.Update(command.Request.Name);

        await unitOfWork.SharedCompleteAsync(cancellationToken);

        return Result.Success();
    }
}
