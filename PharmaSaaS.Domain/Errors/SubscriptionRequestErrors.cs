namespace PharmaSaaS.Domain.Errors;

public record SubscriptionRequestErrors
{
    public static readonly Error NotFound =
        new("SubscriptionRequest.NotFound", "No subscription request found", StatusCodes.NotFound);

    public static readonly Error DuplicatedEmail =
        new("SubscriptionRequest.DuplicatedEmail", "A subscription request with the same email already exists", StatusCodes.Conflict);

    public static readonly Error AlreadyPending =
        new("SubscriptionRequest.AlreadyPending", "A pending request with the same pharmacy name, phone number, and email already exists", StatusCodes.Conflict);

    public static readonly Error AlreadyApproved =
        new("SubscriptionRequest.AlreadyApproved", "A subscription request with these details has already been approved", StatusCodes.Conflict);
}
