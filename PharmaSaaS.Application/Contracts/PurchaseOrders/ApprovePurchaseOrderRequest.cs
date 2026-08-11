using FluentValidation;

namespace PharmaSaaS.Application.Contracts.PurchaseOrders;

public record ApprovePurchaseOrderRequest(
    decimal TotalAmount,
    List<ApprovePurchaseOrderItemRequest> Items
);

public record ApprovePurchaseOrderItemRequest(
    int ProductId, 
    float ReceivedQuantity, 
    decimal UnitCost,
    bool IsMissing = false
);

#region Validation

public class ApprovePurchaseOrderRequestValidator : AbstractValidator<ApprovePurchaseOrderRequest>
{
    public ApprovePurchaseOrderRequestValidator()
    {
        RuleFor(x => x.TotalAmount)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Items)
            .NotEmpty()
            .Must(x => x.Select(i => i.ProductId).Count() == x.Select(i => i.ProductId).Distinct().Count())
            .When(x => x.Items != null && x.Items.Count > 0)
            .WithMessage("Product IDs must be unique within the same order.");

        RuleForEach(x => x.Items).SetValidator(new ApprovePurchaseOrderItemRequestValidator());
    }
}

public class ApprovePurchaseOrderItemRequestValidator : AbstractValidator<ApprovePurchaseOrderItemRequest>
{
    public ApprovePurchaseOrderItemRequestValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.ReceivedQuantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.UnitCost).GreaterThanOrEqualTo(0);
    }
}

#endregion
