namespace PharmaSaaS.Application.Features.AI.SetApiKey;

public sealed class SetAiApiKeyCommandHandler(IUnitOfWork unitOfWork, ICurrentTenantGetter currentTenantGetter) : IRequestHandler<SetAiApiKeyCommand, Result>
{
    public async Task<Result> Handle(SetAiApiKeyCommand command, CancellationToken cancellationToken = default)
    {
        var pharmacyId = currentTenantGetter.GetTenantId()!;

        if (await unitOfWork.Pharmacies.FindByTenantIdAsync(pharmacyId, cancellationToken) is not { } pharmacy)
            return Result.Failure(PharmacyErrors.NotFound);

        pharmacy.SetCustomAiApiKey(command.Request.ApiKey);

        await unitOfWork.SharedCompleteAsync(cancellationToken);

        return Result.Success();
    }
}
