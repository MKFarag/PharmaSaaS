namespace PharmaSaaS.Application.Contracts.Invoices;

public record CreateInvoiceRequest(
    int? CustomerId,
    IList<InvoiceItemRequest> Items
);

public record InvoiceItemRequest(
    int ProductId,
    float Quantity
);

#region Validation

public class CreateInvoiceRequestValidator : AbstractValidator<CreateInvoiceRequest>
{
    public CreateInvoiceRequestValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty()
            .Must(items => items.Select(i => i.ProductId).Distinct().Count() == items.Count)
            .WithMessage("Invoice items must contain unique products. You cannot add the same product multiple times.");
        
        RuleForEach(x => x.Items)
            .ChildRules(item =>
            {
                item.RuleFor(i => i.ProductId).GreaterThan(0);

                item.RuleFor(i => i.Quantity).GreaterThan(0);
            });
    }
}

#endregion
