namespace PharmaSaaS.Application.Features.Payments.Verify;

public sealed class VerifySubscriptionPaymentCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<VerifySubscriptionPaymentCommand, Result>
{
    public async Task<Result> Handle(VerifySubscriptionPaymentCommand command, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.Pharmacies.GetPaymentAsync(command.PaymentId, cancellationToken) is not { } payment)
            return Result.Failure(SubscriptionPaymentErrors.NotFound);

        if (payment.IsVerified)
            return Result.Failure(SubscriptionPaymentErrors.AlreadyVerified);

        var pharmacy = payment.Pharmacy ?? await unitOfWork.Pharmacies.GetAsync(payment.PharmacyId, cancellationToken);
        if (pharmacy is null)
            return Result.Failure(PharmacyErrors.NotFound);

        payment.Verify(command.Request.Amount);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var baseDate = pharmacy.SubscriptionEnd.HasValue && pharmacy.SubscriptionEnd.Value > today
            ? pharmacy.SubscriptionEnd.Value
            : today;

        pharmacy.SetSubscriptionEnd(baseDate.AddMonths(command.Request.ExtendByMonths));

        if (!pharmacy.IsActive)
            pharmacy.ToggleStatus();

        await unitOfWork.SharedCompleteAsync(cancellationToken);

        return Result.Success();
    }
}
