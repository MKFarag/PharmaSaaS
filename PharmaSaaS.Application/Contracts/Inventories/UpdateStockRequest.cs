namespace PharmaSaaS.Application.Contracts.Inventories;

public record UpdateStockRequest(
    float Quantity,
    decimal SellingPrice,
    int? TargetStock,
    bool IsRare
);

public class UpdateStockRequestValidator : AbstractValidator<UpdateStockRequest>
{
    public UpdateStockRequestValidator()
    {
        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.SellingPrice)
            .GreaterThan(0);

        RuleFor(x => x.TargetStock)
            .GreaterThan(0)
            .When(x => x.TargetStock is not null);
    }
}
