using PharmaSaaS.Domain.Entities;
using PharmaSaaS.Domain.Errors;

namespace PharmaSaaS.Application.Interfaces.Services;

/// <summary>
/// Validates that a set of return items can be legally applied to an invoice.
/// Extracted to avoid duplicating identical validation logic across
/// <c>ProcessReturnCommandHandler</c> and <c>CheckReturnImpactQueryHandler</c>.
/// </summary>
public interface IReturnItemValidatorService
{
    /// <summary>
    /// Validates that every requested return item references a product that exists on the
    /// invoice, and that the return quantity does not exceed what was originally purchased
    /// minus what was already returned.
    /// </summary>
    /// <param name="invoice">The invoice being returned against (must have Items loaded).</param>
    /// <param name="existingReturns">All returns previously recorded for this invoice.</param>
    /// <param name="requestItems">The items the caller wants to return now.</param>
    /// <returns>
    /// <c>null</c> when validation passes; an <see cref="Error"/> describing the first
    /// violation otherwise.
    /// </returns>
    Error? Validate(Invoice invoice, IEnumerable<Return> existingReturns, IEnumerable<ReturnItemRequest> requestItems);
}
