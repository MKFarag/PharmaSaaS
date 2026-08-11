using FluentValidation;

namespace PharmaSaaS.Application.Contracts.SupplierPayments;

public record SupplierPaymentRequestFilters
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public int? SupplierId { get; init; }
}

#region Validation

public class SupplierPaymentRequestFiltersValidator : AbstractValidator<SupplierPaymentRequestFilters>
{
    public SupplierPaymentRequestFiltersValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 250);
    }
}

#endregion
