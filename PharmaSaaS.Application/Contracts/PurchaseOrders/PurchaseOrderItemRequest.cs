namespace PharmaSaaS.Application.Contracts.PurchaseOrders;

public record PurchaseOrderItemRequest(
    int ProductId,
    float Quantity
);

#region Validation

public class PurchaseOrderItemRequestValidator : AbstractValidator<PurchaseOrderItemRequest>
{
    public PurchaseOrderItemRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0);

        RuleFor(x => x.Quantity)
            .GreaterThan(0);
    }
}

#endregion
