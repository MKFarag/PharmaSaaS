using FluentValidation;

namespace PharmaSaaS.Application.Contracts.PurchaseOrders;

public record PurchaseOrderRequest(
    int SupplierId,
    List<PurchaseOrderItemRequest> Items
);

public class PurchaseOrderRequestValidator : AbstractValidator<PurchaseOrderRequest>
{
    public PurchaseOrderRequestValidator()
    {
        RuleFor(x => x.SupplierId).GreaterThan(0);
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).SetValidator(new PurchaseOrderItemRequestValidator());
    }
}
