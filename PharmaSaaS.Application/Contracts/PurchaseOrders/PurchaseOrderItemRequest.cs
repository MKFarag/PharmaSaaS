using FluentValidation;

namespace PharmaSaaS.Application.Contracts.PurchaseOrders;

public record PurchaseOrderItemRequest(
    int ProductId,
    float Quantity,
    decimal UnitCost
);

public class PurchaseOrderItemRequestValidator : AbstractValidator<PurchaseOrderItemRequest>
{
    public PurchaseOrderItemRequestValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.UnitCost).GreaterThanOrEqualTo(0);
    }
}
