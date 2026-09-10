namespace PharmaSaaS.Application.Contracts.Expenses;

public record ExpenseRequest(
    decimal Amount,
    string Description
);

#region Validation

public class ExpenseRequestValidator : AbstractValidator<ExpenseRequest>
{
    public ExpenseRequestValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0);

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(500);
    }
}

#endregion
