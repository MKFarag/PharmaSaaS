namespace PharmaSaaS.Domain.Errors;

public record CustomerErrors
{
    public static readonly Error NotFound =
        new("Customer.NotFound", "No customer found", StatusCodes.NotFound);

    public static readonly Error DuplicatedPhoneNumber =
        new("Customer.DuplicatedPhoneNumber", "Another customer with the same phone number already exists", StatusCodes.Conflict);
}
