using PharmaSaaS.Application.Contracts.Customers;
using PharmaSaaS.Application.Features.Customers.Add;
using PharmaSaaS.Application.Features.Customers.Get;
using PharmaSaaS.Application.Features.Customers.GetAll;
using PharmaSaaS.Application.Features.Customers.Update;

namespace PharmaSaaS.Presentation.Controllers;

/// <summary>
/// Manage customers.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
[EnableRateLimiting(RateLimitingOptions.PolicyNames.Concurrency)]
public class CustomersController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Retrieves all customers.
    /// </summary>
    /// <remarks>
    /// Returns a paginated list of all customers.
    /// </remarks>
    /// <param name="filters">Pagination and search filters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated list of customers.</returns>
    /// <response code="200">Returns the list of customers.</response>
    /// <response code="401">If the user is unauthorized.</response>
    [HttpGet("")]
    [HasPermission(Permissions.ReadCustomer)]
    [ProducesResponseType(typeof(IPaginatedList<CustomerResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll([FromQuery] RequestFilters filters, CancellationToken cancellationToken)
        => Ok(await sender.Send(new GetAllCustomersQuery(filters), cancellationToken));

    /// <summary>
    /// Retrieves a specific customer by ID.
    /// </summary>
    /// <remarks>
    /// Retrieves the details of a specific customer based on its unique identifier.
    /// </remarks>
    /// <param name="id">The unique identifier of the customer.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The customer details.</returns>
    /// <response code="200">Returns the customer details.</response>
    /// <response code="404">If the customer is not found.</response>
    /// <response code="401">If the user is unauthorized.</response>
    [HttpGet("{id}")]
    [HasPermission(Permissions.ReadCustomer)]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Get([FromRoute] int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetCustomerByIdQuery(id), cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Creates a new customer.
    /// </summary>
    /// <remarks>
    /// Creates a new customer.
    /// </remarks>
    /// <param name="request">The customer creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The newly created customer id.</returns>
    /// <response code="201">Returns the newly created customer id.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="401">If the user is unauthorized.</response>
    /// <response code="409">If a customer with the same phone number already exists.</response>
    [HttpPost("")]
    [Idempotent]
    [HasPermission(Permissions.AddCustomer)]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Add([FromBody] CustomerRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new AddCustomerCommand(request), cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(Get), new { id = result.Value.Id }, result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Updates an existing customer.
    /// </summary>
    /// <remarks>
    /// Updates the details of an existing customer by ID.
    /// </remarks>
    /// <param name="id">The unique identifier of the customer to update.</param>
    /// <param name="request">The updated customer data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">If the customer was successfully updated.</response>
    /// <response code="400">If the request data is invalid.</response>
    /// <response code="404">If the customer is not found.</response>
    /// <response code="401">If the user is unauthorized.</response>
    /// <response code="409">If another customer with the same phone number already exists.</response>
    [HttpPut("{id}")]
    [HasPermission(Permissions.UpdateCustomer)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] CustomerRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateCustomerCommand(id, request), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.ToProblem();
    }
}
