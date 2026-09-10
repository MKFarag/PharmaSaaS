using StatusCodes = PharmaSaaS.Domain.Constants.StatusCodes;

namespace PharmaSaaS.Domain.Errors;

public record ExpenseErrors
{
    public static readonly Error NotFound =
        new("Expense.NotFound", "The expense was not found.", StatusCodes.NotFound);
}
