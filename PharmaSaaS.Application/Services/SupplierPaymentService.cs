namespace PharmaSaaS.Application.Services;

public sealed class SupplierPaymentService : ISupplierPaymentService
{
    public void DistributePaymentAmongOrders(decimal amount, IEnumerable<PurchaseOrder> orders)
    {
        decimal remainingPayment = amount;

        foreach (var order in orders.OrderBy(o => o.OrderDate))
        {
            if (remainingPayment <= 0)
                break;

            decimal orderRemainingAmount = order.RemainingAmount;

            if (orderRemainingAmount > 0)
            {
                decimal amountToApply = Math.Min(remainingPayment, orderRemainingAmount);
                order.RecordPayment(amountToApply);
                remainingPayment -= amountToApply;
            }
        }
    }
}
