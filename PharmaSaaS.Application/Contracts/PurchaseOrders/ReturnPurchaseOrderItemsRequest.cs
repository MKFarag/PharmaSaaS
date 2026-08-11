using FluentValidation;

namespace PharmaSaaS.Application.Contracts.PurchaseOrders;

public record ReturnPurchaseOrderItemsRequest(List<ReturnPurchaseOrderItemRequest> Items);

public record ReturnPurchaseOrderItemRequest(
    int ProductId, 
    float QuantityToReturn, 
    string? ReturnReason
);

#region Validation

public class ReturnPurchaseOrderItemsRequestValidator : AbstractValidator<ReturnPurchaseOrderItemsRequest>
{
    public ReturnPurchaseOrderItemsRequestValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty()
            .Must(x => x.Select(i => i.ProductId).Count() == x.Select(i => i.ProductId).Distinct().Count())
            .When(x => x.Items != null && x.Items.Count > 0)
            .WithMessage("Product IDs must be unique within the same order.");

        RuleForEach(x => x.Items).SetValidator(new ReturnPurchaseOrderItemRequestValidator());
    }
}

public class ReturnPurchaseOrderItemRequestValidator : AbstractValidator<ReturnPurchaseOrderItemRequest>
{
    public ReturnPurchaseOrderItemRequestValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.QuantityToReturn).GreaterThan(0);
        RuleFor(x => x.ReturnReason).MaximumLength(500);
    }
}

#endregion
