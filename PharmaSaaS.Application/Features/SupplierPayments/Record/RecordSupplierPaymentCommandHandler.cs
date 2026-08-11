using Mapster;
using PharmaSaaS.Application.Contracts.SupplierPayments;
using PharmaSaaS.Domain.Errors;

namespace PharmaSaaS.Application.Features.SupplierPayments.Record;

public sealed class RecordSupplierPaymentCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<RecordSupplierPaymentCommand, Result<SupplierPaymentResponse>>
{
    public async Task<Result<SupplierPaymentResponse>> Handle(RecordSupplierPaymentCommand command, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.Suppliers.GetAsync([command.Request.SupplierId], cancellationToken) is not { } supplier)
            return Result.Failure<SupplierPaymentResponse>(SupplierErrors.NotFound);

        var orders = (await unitOfWork.PurchaseOrders
            .TrackedFindAllAsync(po => command.Request.OrderIds.Contains(po.Id), cancellationToken)).ToList();

        if (orders.Count != command.Request.OrderIds.Distinct().Count() 
            || orders.Any(o => o.SupplierId != command.Request.SupplierId))
            return Result.Failure<SupplierPaymentResponse>(SupplierPaymentErrors.OrderNotBelongToSupplier);

        var payment = SupplierPayment.Create(
            command.Request.SupplierId,
            command.Request.Amount,
            command.Request.Notes);

        payment.SetOrders(orders);

        await unitOfWork.SupplierPayments.AddAsync(payment, cancellationToken);

        decimal remainingPayment = command.Request.Amount;
        foreach (var order in orders.OrderBy(o => o.OrderDate))
        {
            if (remainingPayment <= 0) break;

            decimal orderRemainingAmount = order.RemainingAmount;
            if (orderRemainingAmount > 0)
            {
                decimal amountToApply = Math.Min(remainingPayment, orderRemainingAmount);
                order.RecordPayment(amountToApply);
                remainingPayment -= amountToApply;
            }
        }

        supplier.RecordPayment(command.Request.Amount);

        payment.SetSupplier(supplier);

        await unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success(new SupplierPaymentResponse(
            payment.Id,
            supplier.Id,
            supplier.Name,
            payment.Amount,
            payment.PaidAt,
            payment.Notes,
            [.. orders.Select(o => o.Id)]
        ));
    }
}
