using FluentValidation;

namespace PharmaSaaS.Application.Contracts.PurchaseOrders;

public record PurchaseOrderRequest(
    int SupplierId, 
    List<PurchaseOrderItemRequest> Items, 
    decimal? ExpectedTotalAmount
);



#region Validation

public class PurchaseOrderRequestValidator : AbstractValidator<PurchaseOrderRequest>
{
    public PurchaseOrderRequestValidator()
    {
        RuleFor(x => x.SupplierId).GreaterThan(0);
        
        RuleFor(x => x.Items)
            .NotEmpty()
            .Must(x => x.Select(i => i.ProductId).Count() == x.Select(i => i.ProductId).Distinct().Count())
            .When(x => x.Items != null && x.Items.Count > 0)
            .WithMessage("Product IDs must be unique within the same order.");

        RuleForEach(x => x.Items).SetValidator(new PurchaseOrderItemRequestValidator());
        
        RuleFor(x => x.ExpectedTotalAmount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.ExpectedTotalAmount.HasValue);
    }
}



#endregion
