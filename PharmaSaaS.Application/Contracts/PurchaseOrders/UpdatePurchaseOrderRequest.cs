namespace PharmaSaaS.Application.Contracts.PurchaseOrders;

public record UpdatePurchaseOrderRequest(
    decimal? ExpectedTotalAmount,
    List<PurchaseOrderItemRequest>? AddedItems
);

#region Validation

public class UpdatePurchaseOrderRequestValidator : AbstractValidator<UpdatePurchaseOrderRequest>
{
    public UpdatePurchaseOrderRequestValidator()
    {
        RuleFor(x => x.ExpectedTotalAmount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.ExpectedTotalAmount.HasValue);

        RuleFor(x => x.AddedItems)
            .Must(x => x!.Select(i => i.ProductId).Count() == x!.Select(i => i.ProductId).Distinct().Count())
            .When(x => x.AddedItems != null && x.AddedItems.Count > 0)
            .WithMessage("Added product IDs must be unique.");

        RuleForEach(x => x.AddedItems)
            .SetValidator(new PurchaseOrderItemRequestValidator())
            .When(x => x.AddedItems != null);
    }
}

#endregion
