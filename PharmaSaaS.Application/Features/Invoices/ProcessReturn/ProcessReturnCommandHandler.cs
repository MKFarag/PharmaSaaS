namespace PharmaSaaS.Application.Features.Invoices.ProcessReturn;

public sealed class ProcessReturnCommandHandler(IUnitOfWork unitOfWork, IReturnService returnService, ICustomerLoyaltyService customerLoyaltyService)
    : IRequestHandler<ProcessReturnCommand, Result<ProcessReturnResponse>>
{
    public async Task<Result<ProcessReturnResponse>> Handle(ProcessReturnCommand command, CancellationToken cancellationToken = default)
    {
        string[] includes = [nameof(Invoice.Items), nameof(Invoice.Returns), nameof(Invoice.Customer)];

        if (await unitOfWork.Invoices.TrackedFindAsync(i => i.Id == command.InvoiceId, includes, cancellationToken) is not { } invoice)
            return Result.Failure<ProcessReturnResponse>(InvoiceErrors.NotFound);

        if (returnService.ValidateReturnRequest(invoice, invoice.Returns, command.Request.Items) is { } validationError)
            return Result.Failure<ProcessReturnResponse>(validationError);

        if (await unitOfWork.Shifts.TrackedFindAsync(s => s.Status == ShiftStatus.Open, cancellationToken) is not { } currentShift)
            return Result.Failure<ProcessReturnResponse>(InvoiceErrors.ShiftNotOpen);

        var returnItemEntries = returnService.BuildEntries(invoice, command.Request.Items);

        var refundResult = await returnService.CalculateRefundAsync(
            invoice, invoice.Returns, returnItemEntries, invoice.Customer, cancellationToken);

        var newReturns = returnItemEntries
            .Select(entry => Return.Create(invoice.Id, entry.ProductId, (int)entry.Quantity, refundResult.ItemRefunds[entry.ProductId]))
            .ToList();

        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            await unitOfWork.Returns.AddRangeAsync(newReturns, cancellationToken);

            var inventories = await unitOfWork.Inventories
                .TrackedFindAllAsync
                (
                    inv => command.Request.Items.Select(x => x.ProductId).Contains(inv.ProductId),
                    cancellationToken
                );

            foreach (var req in command.Request.Items)
                inventories.FirstOrDefault(i => i.ProductId == req.ProductId)?.AddStock((int)req.Quantity);

            currentShift.AddReturn(refundResult.TotalRefund);

            invoice.MarkHasReturn();

            if (invoice.Customer is { } customer)
                await customerLoyaltyService.UpdateCustomerLoyaltyAsync(customer, -refundResult.TotalRefund, cancellationToken);

            await unitOfWork.CompleteAsync(cancellationToken);

            await unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result.Success(new ProcessReturnResponse(refundResult.TotalRefund, refundResult.DiscountClawback));
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}
