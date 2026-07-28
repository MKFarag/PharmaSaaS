using Microsoft.AspNetCore.Mvc;
using PharmaSaaS.Application.Contracts.Common;
using PharmaSaaS.Application.Contracts.Invoices;
using PharmaSaaS.Application.Features.Invoices.Create;
using PharmaSaaS.Application.Features.Invoices.GetAll;
using PharmaSaaS.Application.Features.Invoices.GetAllReturns;
using PharmaSaaS.Application.Features.Invoices.GetById;
using PharmaSaaS.Application.Features.Invoices.GetReturnById;
using PharmaSaaS.Application.Features.Invoices.ProcessReturn;
using PharmaSaaS.Domain.Constants;
using PharmaSaaS.Presentation.Attributes;

namespace PharmaSaaS.Presentation.Controllers;

/// <summary>
/// Manage Invoices and Returns.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public sealed class InvoicesController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Create a new sale invoice
    /// </summary>
    /// <param name="request">Invoice details</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>Result of the operation</returns>
    /// <response code="200">Invoice successfully created</response>
    /// <response code="400">Validation or business logic error</response>
    [HttpPost]
    [HasPermission(Permissions.CreateInvoice)]
    [Idempotent]
    public async Task<IActionResult> CreateInvoice([FromBody] CreateInvoiceRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateInvoiceCommand(request, User.GetId()!);
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Process a return for an existing invoice
    /// </summary>
    /// <param name="id">Invoice ID</param>
    /// <param name="request">Return items</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>Result of the operation</returns>
    /// <response code="200">Return successfully processed</response>
    /// <response code="400">Validation or business logic error</response>
    [HttpPost("{id}/return")]
    [HasPermission(Permissions.ProcessReturn)]
    [Idempotent]
    public async Task<IActionResult> ProcessReturn([FromRoute] int id, [FromBody] ProcessReturnRequest request, CancellationToken cancellationToken)
    {
        var command = new ProcessReturnCommand(id, request, User.GetId()!);
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get a paginated list of invoices
    /// </summary>
    /// <param name="filters">Pagination and filtering parameters</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>Paginated list of invoices</returns>
    /// <response code="200">Returns list of invoices</response>
    [HttpGet]
    [HasPermission(Permissions.ReadInvoice)]
    public async Task<IActionResult> GetAllInvoices([FromQuery] InvoiceRequestFilters filters, CancellationToken cancellationToken)
    {
        var query = new GetAllInvoicesQuery(filters);
        var result = await sender.Send(query, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get an invoice by ID
    /// </summary>
    /// <param name="id">Invoice ID</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>Invoice details</returns>
    /// <response code="200">Returns invoice details</response>
    /// <response code="404">Invoice not found</response>
    [HttpGet("{id}")]
    [HasPermission(Permissions.ReadInvoice)]
    public async Task<IActionResult> GetInvoiceById([FromRoute] int id, CancellationToken cancellationToken)
    {
        var query = new GetInvoiceByIdQuery(id);
        var result = await sender.Send(query, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get a paginated list of return invoices
    /// </summary>
    /// <param name="filters">Pagination and filtering parameters</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>Paginated list of return invoices</returns>
    /// <response code="200">Returns list of return invoices</response>
    [HttpGet("returns")]
    [HasPermission(Permissions.ReadInvoice)]
    public async Task<IActionResult> GetAllReturnInvoices([FromQuery] ReturnInvoiceRequestFilters filters, CancellationToken cancellationToken)
    {
        var query = new GetAllReturnInvoicesQuery(filters);
        var result = await sender.Send(query, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get a return invoice by ID
    /// </summary>
    /// <param name="id">Return Invoice ID</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>Return invoice details</returns>
    /// <response code="200">Returns return invoice details</response>
    /// <response code="404">Return invoice not found</response>
    [HttpGet("returns/{id}")]
    [HasPermission(Permissions.ReadInvoice)]
    public async Task<IActionResult> GetReturnInvoiceById([FromRoute] int id, CancellationToken cancellationToken)
    {
        var query = new GetReturnInvoiceByIdQuery(id);
        var result = await sender.Send(query, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }
}
