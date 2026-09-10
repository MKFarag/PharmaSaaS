namespace PharmaSaaS.Application.Contracts.Customers;

public record CustomerRequest(
    string FullName,
    string PhoneNumber
);

#region Validation

public class CustomerRequestValidator : AbstractValidator<CustomerRequest>
{
    public CustomerRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .MaximumLength(20);
    }
}

#endregion
