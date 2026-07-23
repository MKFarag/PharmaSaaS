namespace PharmaSaaS.Application.Features.ExpiryReports.Revert;

public sealed class RevertExpiryReportCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<RevertExpiryReportCommand, Result>
{
    public async Task<Result> Handle(RevertExpiryReportCommand command, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.ExpiryReports.GetAsync([command.Id], cancellationToken) is not { } expiryReport)
            return Result.Failure(ExpiryReportErrors.NotFound);

        if (await unitOfWork.Inventories.FindAsync(x => x.ProductId == expiryReport.ProductId, cancellationToken) is { } inventory)
            inventory.AddStock(expiryReport.ExpiredQuantity);

        unitOfWork.ExpiryReports.Delete(expiryReport);

        await unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
