using PharmaSaaS.Application.Contracts.SupplierPayments;
using PharmaSaaS.Application.Features.SupplierPayments.List;
using PharmaSaaS.Application.Features.SupplierPayments.Record;

namespace PharmaSaaS.Presentation.Controllers;

/// <summary>Manage Supplier Payments.</summary>
[Route("api/supplier-payments")]
[ApiController]
[Authorize]
[EnableRateLimiting(RateLimitingOptions.PolicyNames.Concurrency)]
public class SupplierPaymentsController(ISender sender) : ControllerBase
{
    /// <summary>List Supplier Payments</summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/supplier-payments?supplierId=1
    ///
    /// </remarks>
    /// <param name="filters">The filters for the supplier payment list.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated list of supplier payments.</returns>
    /// <response code="200">Returns the paginated list of supplier payments.</response>
    /// <response code="401">If the user is unauthorized.</response>
    [HttpGet]
    [HasPermission(Permissions.ReadSupplier)]
    [ProducesResponseType(typeof(IPaginatedList<SupplierPaymentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> List(
        [FromQuery] SupplierPaymentRequestFilters filters,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new ListSupplierPaymentsQuery(filters), cancellationToken);
        return Ok(result);
    }

    /// <summary>Record Supplier Payment</summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/supplier-payments
    ///     {
    ///        "supplierId": 1,
    ///        "amount": 500.00,
    ///        "orderIds": [1, 2],
    ///        "notes": "Partial payment for invoices #1 and #2"
    ///     }
    ///
    /// </remarks>
    /// <param name="request">The supplier payment payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created supplier payment.</returns>
    /// <response code="200">Returns the created supplier payment.</response>
    /// <response code="400">If the request is invalid or an order doesn't belong to the supplier.</response>
    /// <response code="404">If the supplier is not found.</response>
    /// <response code="401">If the user is unauthorized.</response>
    [HttpPost]
    [Idempotent]
    [HasPermission(Permissions.ManagePurchaseOrder)]
    [ProducesResponseType(typeof(SupplierPaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Record([FromBody] SupplierPaymentRequest request, CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new RecordSupplierPaymentCommand(request), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }
}
