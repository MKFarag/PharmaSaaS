using PharmaSaaS.Application.Contracts.Invoices;
using PharmaSaaS.Application.Features.Invoices.CheckReturnImpact;
using PharmaSaaS.Application.Features.Invoices.Create;
using PharmaSaaS.Application.Features.Invoices.GetAll;
using PharmaSaaS.Application.Features.Invoices.GetById;
using PharmaSaaS.Application.Features.Invoices.ProcessReturn;

namespace PharmaSaaS.Presentation.Controllers;

/// <summary>
/// Manage Invoices and Returns.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
[EnableRateLimiting(RateLimitingOptions.PolicyNames.Concurrency)]
public sealed class InvoicesController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Create a new sale invoice.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/Invoices
    ///     {
    ///       "customerId": 1,
    ///       "items": [ { "productId": 5, "quantity": 2 } ]
    ///     }
    /// </remarks>
    /// <param name="request">Invoice details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Newly created invoice details.</returns>
    /// <response code="200">Invoice successfully created.</response>
    /// <response code="400">Validation or business logic error.</response>
    /// <response code="401">Unauthorized.</response>
    [HttpPost("")]
    [HasPermission(Permissions.CreateInvoice)]
    [Idempotent]
    [ProducesResponseType(typeof(InvoiceDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateInvoice([FromBody] CreateInvoiceRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateInvoiceCommand(request);
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Process a return for an existing invoice.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/Invoices/1/return
    ///     {
    ///       "items": [ { "productId": 5, "quantity": 1 } ]
    ///     }
    /// </remarks>
    /// <param name="id">Invoice ID.</param>
    /// <param name="request">Return items.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Refund amount and discount clawback.</returns>
    /// <response code="200">Return successfully processed.</response>
    /// <response code="400">Validation or business logic error.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="404">Invoice not found.</response>
    [HttpPost("{id}/return")]
    [HasPermission(Permissions.ProcessReturn)]
    [Idempotent]
    [ProducesResponseType(typeof(ProcessReturnResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ProcessReturn([FromRoute] int id, [FromBody] ProcessReturnRequest request, CancellationToken cancellationToken)
    {
        var command = new ProcessReturnCommand(id, request);
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get a paginated list of invoices.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/Invoices?pageNumber=1&amp;pageSize=10
    /// </remarks>
    /// <param name="filters">Pagination and filtering parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paginated list of invoices.</returns>
    /// <response code="200">Returns list of invoices.</response>
    /// <response code="401">Unauthorized.</response>
    [HttpGet("")]
    [HasPermission(Permissions.ReadInvoice)]
    [ProducesResponseType(typeof(IPaginatedList<InvoiceResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllInvoices([FromQuery] InvoiceRequestFilters filters, CancellationToken cancellationToken)
    {
        var query = new GetAllInvoicesQuery(filters);
        var result = await sender.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get an invoice by ID.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/Invoices/1
    /// </remarks>
    /// <param name="id">Invoice ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Invoice details.</returns>
    /// <response code="200">Returns invoice details.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="404">Invoice not found.</response>
    [HttpGet("{id}")]
    [HasPermission(Permissions.ReadInvoice)]
    [ProducesResponseType(typeof(InvoiceDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInvoiceById([FromRoute] int id, CancellationToken cancellationToken)
    {
        var query = new GetInvoiceByIdQuery(id);
        var result = await sender.Send(query, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }
    /// <summary>
    /// Preview the financial and loyalty impact of a return without committing it
    /// </summary>
    /// <remarks>
    /// Use this endpoint before processing a return to inform the customer whether:
    /// - Their remaining purchase total will still qualify for the loyalty discount.
    /// - They will drop below their current loyalty tier threshold.
    ///
    /// Sample request:
    ///
    ///     GET /api/invoices/42/return/preview
    ///     {
    ///       "items": [
    ///         { "productId": 7, "quantity": 1 }
    ///       ]
    ///     }
    /// </remarks>
    /// <param name="id">Invoice ID to preview the return against.</param>
    /// <param name="request">Items the customer intends to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Estimated refund, discount clawback, and tier-drop warning.</returns>
    /// <response code="200">Preview calculated successfully.</response>
    /// <response code="400">One or more return items are invalid or exceed purchased quantity.</response>
    /// <response code="404">Invoice not found.</response>
    [HttpGet("{id}/return/preview")]
    [HasPermission(Permissions.PreviewReturn)]
    [ProducesResponseType(typeof(CheckReturnImpactResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PreviewReturnImpact([FromRoute] int id, [FromQuery] ProcessReturnRequest request, CancellationToken cancellationToken)
    {
        var query = new CheckReturnImpactQuery(id, request);
        var result = await sender.Send(query, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

}
