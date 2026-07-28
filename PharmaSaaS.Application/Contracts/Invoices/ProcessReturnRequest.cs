namespace PharmaSaaS.Application.Contracts.Invoices;

public record ProcessReturnRequest(
    IList<ReturnItemRequest> Items
);

public record ReturnItemRequest(
    int ProductId,
    float Quantity
);

#region Validation

public class ProcessReturnRequestValidator : AbstractValidator<ProcessReturnRequest>
{
    public ProcessReturnRequestValidator()
    {
        RuleFor(x => x.Items).NotEmpty();
        
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).GreaterThan(0);

            item.RuleFor(i => i.Quantity).GreaterThan(0);
        });
    }
}

#endregion
