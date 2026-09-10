namespace PharmaSaaS.Application.Features.Invoices.Create;

public sealed class CreateInvoiceCommandHandler(IUnitOfWork unitOfWork, IInvoiceService invoiceService, ICustomerLoyaltyService customerLoyaltyService)
    : IRequestHandler<CreateInvoiceCommand, Result<InvoiceDetailResponse>>
{
    public async Task<Result<InvoiceDetailResponse>> Handle(CreateInvoiceCommand command, CancellationToken cancellationToken)
    {
        if (await unitOfWork.Shifts.TrackedFindAsync(s => s.Status == ShiftStatus.Open, cancellationToken) is not { } shift)
            return Result.Failure<InvoiceDetailResponse>(InvoiceErrors.ShiftNotOpen);

        var productIds = command.Request.Items.Select(x => x.ProductId).ToList();

        var inventories = (await unitOfWork.Inventories
            .TrackedFindAllAsync
            (
                inv => productIds.Contains(inv.ProductId),
                [nameof(Inventory.Product)],
                cancellationToken
            )).ToList();

        if (inventories.Count != productIds.Count)
            return Result.Failure<InvoiceDetailResponse>(InvoiceErrors.ProductNotFound);

        var totalResult = invoiceService.CalculateTotalAndValidateStock(command.Request.Items, inventories);

        if (totalResult.IsFailure)
            return Result.Failure<InvoiceDetailResponse>(totalResult.Error);

        decimal total = totalResult.Value;

        decimal discountAmount = 0;
        Customer? customer = null;

        if (command.Request.CustomerId.HasValue)
        {
            customer = await unitOfWork.Customers.GetAsync([command.Request.CustomerId.Value], cancellationToken);

            if (customer is { })
                discountAmount = await customerLoyaltyService.CalculateDiscountAsync(customer, total, cancellationToken);
        }

        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var invoice = Invoice.Create(shift.Id, customer?.Id, total, discountAmount, command.Request.Items.Count);

            await unitOfWork.Invoices.AddAsync(invoice, cancellationToken);

            await unitOfWork.CompleteAsync(cancellationToken);

            var invoiceItems = invoiceService
                .ProcessInvoiceItemsAndDeductStock(invoice.Id, command.Request.Items, inventories);

            await unitOfWork.InvoiceItems.AddRangeAsync(invoiceItems, cancellationToken);

            shift.AddSale(invoice.FinalAmount);

            if (customer is { })
                await customerLoyaltyService.UpdateCustomerLoyaltyAsync(customer, invoice.FinalAmount, cancellationToken);

            await unitOfWork.CompleteAsync(cancellationToken);

            await unitOfWork.CommitTransactionAsync(cancellationToken);

            var savedInvoice = await unitOfWork.Invoices.FindAsync(i => i.Id == invoice.Id, Invoice.DetailIncludes, cancellationToken);

            return Result.Success(savedInvoice!.Adapt<InvoiceDetailResponse>());
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}
