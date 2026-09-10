namespace PharmaSaaS.Application.Contracts.Inventories;

public record AdjustStockRequest(
    float Amount
);

#region Validation

public class AdjustStockRequestValidator : AbstractValidator<AdjustStockRequest>
{
    public AdjustStockRequestValidator()
    {
        RuleFor(x => x.Amount)
            .NotEqual(0)
            .WithMessage("Adjustment amount cannot be zero.");
    }
}

#endregion