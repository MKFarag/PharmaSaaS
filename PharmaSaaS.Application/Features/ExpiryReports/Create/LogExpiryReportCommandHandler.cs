namespace PharmaSaaS.Application.Features.ExpiryReports.Create;

public sealed class LogExpiryReportCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<LogExpiryReportCommand, Result<ExpiryReportResponse>>
{
    public async Task<Result<ExpiryReportResponse>> Handle(LogExpiryReportCommand command, CancellationToken cancellationToken = default)
    {
        string[] includes = [
            nameof(Inventory.Product),
            $"{nameof(Inventory.Product)}.{nameof(Product.Category)}",
            $"{nameof(Inventory.Product)}.{nameof(Product.DosageForm)}"
        ];

        if (await unitOfWork.Inventories.TrackedFindAsync(i => i.ProductId == command.Request.ProductId, includes, cancellationToken) is not { } inventory)
            return Result.Failure<ExpiryReportResponse>(InventoryErrors.ProductNotFound);

        if (!inventory.TryDeductStock(command.Request.ExpiredQuantity))
            return Result.Failure<ExpiryReportResponse>(InventoryErrors.QuantityExceedsStock);

        var expiryReport = ExpiryReport.Create(
            command.Request.ProductId,
            command.Request.ExpiredQuantity,
            command.UserId,
            command.Request.Notes);

        await unitOfWork.ExpiryReports.AddAsync(expiryReport, cancellationToken);

        await unitOfWork.CompleteAsync(cancellationToken);

        var response = new ExpiryReportResponse(
            expiryReport.Id,
            expiryReport.ExpiredQuantity,
            expiryReport.ReportDate,
            expiryReport.Notes,
            expiryReport.ReportedByEmployeeId,
            inventory.Product.Adapt<ProductResponse>()
        );

        return Result.Success(response);
    }
}
