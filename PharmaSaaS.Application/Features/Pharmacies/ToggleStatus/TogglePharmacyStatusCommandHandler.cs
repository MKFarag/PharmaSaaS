namespace PharmaSaaS.Application.Features.Pharmacies.ToggleStatus;

public sealed class TogglePharmacyStatusCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<TogglePharmacyStatusCommand, Result>
{
    public async Task<Result> Handle(TogglePharmacyStatusCommand command, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.Pharmacies.GetAsync(command.Id, cancellationToken) is not { } pharmacy)
            return Result.Failure(PharmacyErrors.NotFound);

        pharmacy.ToggleStatus();

        await unitOfWork.SharedCompleteAsync(cancellationToken);

        return Result.Success();
    }
}
