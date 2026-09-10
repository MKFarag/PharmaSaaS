namespace PharmaSaaS.Application.Contracts.ExpiryReports;

public record ExpiryReportRequest(
    int ProductId,
    float ExpiredQuantity,
    string? Notes
);

#region Validation

public class ExpiryReportRequestValidator : AbstractValidator<ExpiryReportRequest>
{
    public ExpiryReportRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0);

        RuleFor(x => x.ExpiredQuantity)
            .GreaterThan(0);

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .When(x => x.Notes is not null);
    }
}

#endregion
