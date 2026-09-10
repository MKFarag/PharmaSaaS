namespace PharmaSaaS.Application.Interfaces.Services;

using PharmaSaaS.Domain.Entities;

public interface ISupplierPaymentService
{
    void DistributePaymentAmongOrders(decimal amount, IEnumerable<PurchaseOrder> orders);
}
