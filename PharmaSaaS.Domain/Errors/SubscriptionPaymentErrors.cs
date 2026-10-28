namespace PharmaSaaS.Domain.Errors;

public record SubscriptionPaymentErrors
{
    public static readonly Error NotFound =
        new("SubscriptionPayment.NotFound", "No subscription payment found", StatusCodes.NotFound);

    public static readonly Error AlreadyVerified =
        new("SubscriptionPayment.AlreadyVerified", "This subscription payment has already been verified", StatusCodes.Conflict);
}
