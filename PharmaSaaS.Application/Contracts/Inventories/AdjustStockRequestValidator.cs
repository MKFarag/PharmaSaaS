namespace PharmaSaaS.Application.Contracts.Inventories;

public sealed class AdjustStockRequestValidator : AbstractValidator<AdjustStockRequest>
{
    public AdjustStockRequestValidator()
    {
        RuleFor(x => x.Amount)
            .NotEqual(0).WithMessage("Adjustment amount cannot be zero.");
    }
}
