namespace PharmaSaaS.Application.Features.Invoices.CheckReturnImpact;

public sealed class CheckReturnImpactQueryHandler(IUnitOfWork unitOfWork, IReturnService returnService)
    : IRequestHandler<CheckReturnImpactQuery, Result<CheckReturnImpactResponse>>
{
    public async Task<Result<CheckReturnImpactResponse>> Handle(CheckReturnImpactQuery query, CancellationToken cancellationToken = default)
    {
        string[] includes =
        [
            nameof(Invoice.Items),
            nameof(Invoice.Returns),
            nameof(Invoice.Customer),
            $"{nameof(Invoice.Customer)}.{nameof(Customer.CurrentTier)}"
        ];

        if (await unitOfWork.Invoices.FindAsync(i => i.Id == query.InvoiceId, includes, cancellationToken) is not { } invoice)
            return Result.Failure<CheckReturnImpactResponse>(InvoiceErrors.NotFound);

        if (returnService.ValidateReturnRequest(invoice, invoice.Returns, query.Request.Items) is { } validationError)
            return Result.Failure<CheckReturnImpactResponse>(validationError);

        var returnItemEntries = returnService.BuildEntries(invoice, query.Request.Items);

        var refundResult = await returnService
            .CalculateRefundAsync(invoice, invoice.Returns, returnItemEntries, invoice.Customer, cancellationToken);

        var currentTier = invoice.Customer?.CurrentTier;
        var projectedSpend = invoice.Customer is { }
            ? Math.Max(0, invoice.Customer.TotalSpend - refundResult.TotalRefund)
            : 0m;

        return Result.Success(new CheckReturnImpactResponse(
            EstimatedRefund: refundResult.TotalRefund,
            DiscountClawback: refundResult.DiscountClawback,
            WillDropBelowLoyaltyTier: currentTier is { } && projectedSpend < currentTier.SpendThreshold,
            CurrentTierName: currentTier?.Name,
            CurrentTierThreshold: currentTier?.SpendThreshold,
            ProjectedSpendAfterReturn: projectedSpend
        ));
    }
}
