namespace PharmaSaaS.Application.Features.ExpiryReports.Create;

public sealed class LogExpiryReportCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<LogExpiryReportCommand, Result>
{
    public async Task<Result> Handle(LogExpiryReportCommand command, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.Inventories.FindAsync(i => i.ProductId == command.Request.ProductId, cancellationToken) is not { } inventory)
            return Result.Failure(InventoryErrors.ProductNotFound);

        if (command.Request.ExpiredQuantity > inventory.Quantity)
            return Result.Failure(InventoryErrors.QuantityExceedsStock);

        var expiryReport = ExpiryReport.Create(
            command.Request.ProductId,
            command.Request.ExpiredQuantity,
            command.UserId,
            command.Request.Notes);

        await unitOfWork.ExpiryReports.AddAsync(expiryReport, cancellationToken);

        inventory.DeductStock(command.Request.ExpiredQuantity);

        await unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
