using FluentValidation;

namespace PharmaSaaS.Application.Contracts.SupplierPayments;

public record SupplierPaymentRequest(
    int SupplierId,
    decimal Amount,
    List<int> OrderIds,
    string? Notes
);

#region Validation

public class SupplierPaymentRequestValidator : AbstractValidator<SupplierPaymentRequest>
{
    public SupplierPaymentRequestValidator()
    {
        RuleFor(x => x.SupplierId)
            .GreaterThan(0);

        RuleFor(x => x.Amount)
            .GreaterThan(0);

        RuleFor(x => x.OrderIds)
            .NotEmpty()
            .Must(x => x.Count == x.Distinct().Count())
            .When(x => x.OrderIds != null && x.OrderIds.Count > 0)
            .WithMessage("Order IDs must be unique.");
    }
} 

#endregion
