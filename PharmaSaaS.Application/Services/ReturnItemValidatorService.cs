using PharmaSaaS.Application.Interfaces.Services;
using PharmaSaaS.Domain.Entities;
using PharmaSaaS.Domain.Errors;

namespace PharmaSaaS.Application.Services;

/// <summary>
/// Validates return items against an invoice in a single pass:
/// checks that every product exists on the invoice and that the requested
/// quantity does not exceed what is still returnable.
/// </summary>
public sealed class ReturnItemValidatorService : IReturnItemValidatorService
{
    public Error? Validate(Invoice invoice, IEnumerable<Return> existingReturns, IEnumerable<ReturnItemRequest> requestItems)
    {
        // Build a lookup from the existing returns so we only scan them once
        var returnedByProduct = existingReturns
            .GroupBy(r => r.ProductId)
            .ToDictionary(g => g.Key, g => g.Sum(r => r.Quantity));

        foreach (var reqItem in requestItems)
        {
            if (invoice.Items.FirstOrDefault(i => i.ProductId == reqItem.ProductId) is not { } originalItem)
                return InvoiceErrors.ProductNotInInvoice;

            var alreadyReturned = returnedByProduct.GetValueOrDefault(reqItem.ProductId, 0);

            if (alreadyReturned + reqItem.Quantity > originalItem.Quantity)
                return InvoiceErrors.ReturnQuantityExceeded;
        }

        return null;
    }
}
