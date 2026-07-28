using FluentValidation;

namespace PharmaSaaS.Application.Contracts.Invoices;

public record ReturnInvoiceRequestFilters
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public int? ShiftId { get; init; }
    public int? InvoiceId { get; init; }
    public DateOnly? Date { get; init; }
    public int? CustomerId { get; init; }
}

public class ReturnInvoiceRequestFiltersValidator : AbstractValidator<ReturnInvoiceRequestFilters>
{
    public ReturnInvoiceRequestFiltersValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 250);
    }
}
