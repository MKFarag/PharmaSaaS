namespace PharmaSaaS.Application.Features.Pharmacies.SetSubscription;

public class SetPharmacySubscriptionCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<SetPharmacySubscriptionCommand, Result>
{
    public async Task<Result> Handle(SetPharmacySubscriptionCommand command, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.Pharmacies.GetAsync(command.Id, cancellationToken) is not { } pharmacy)
            return Result.Failure(PharmacyErrors.NotFound);

        pharmacy.SetSubscriptionEnd(command.Request.End);

        await unitOfWork.SharedCompleteAsync(cancellationToken);

        return Result.Success();
    }
}
