using PharmaSaaS.Application.Contracts.PurchaseOrders;
using PharmaSaaS.Application.Features.PurchaseOrders.Approve;
using PharmaSaaS.Application.Features.PurchaseOrders.Create;
using PharmaSaaS.Application.Features.PurchaseOrders.Delete;
using PharmaSaaS.Application.Features.PurchaseOrders.Get;
using PharmaSaaS.Application.Features.PurchaseOrders.GetItems;
using PharmaSaaS.Application.Features.PurchaseOrders.List;
using PharmaSaaS.Application.Features.PurchaseOrders.Return;
using PharmaSaaS.Application.Features.PurchaseOrders.Update;

namespace PharmaSaaS.Presentation.Controllers;

/// <summary>Manage Purchase Orders.</summary>
[Route("api/purchase-orders")]
[ApiController]
[Authorize]
[EnableRateLimiting(RateLimitingOptions.PolicyNames.Concurrency)]
public sealed class PurchaseOrdersController(ISender sender) : ControllerBase
{
    /// <summary>List Purchase Orders</summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     GET /api/purchase-orders?supplierId=1
    /// 
    /// </remarks>
    /// <param name="filters">Filters for listing purchase orders</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A paginated list of purchase orders.</returns>
    /// <response code="200">Returns the paginated list of purchase orders</response>
    [HttpGet]
    [HasPermission(Permissions.ReadPurchaseOrder)]
    [ProducesResponseType(typeof(IPaginatedList<PurchaseOrderResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] PurchaseOrderRequestFilters filters,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new ListPurchaseOrdersQuery(filters), cancellationToken);
        return Ok(result);
    }

    /// <summary>Purchase Order Detail</summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     GET /api/purchase-orders/1
    /// 
    /// </remarks>
    /// <param name="id">The purchase order ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Full details of a purchase order including all items.</returns>
    /// <response code="200">Returns the purchase order detail</response>
    /// <response code="404">If the purchase order is not found</response>
    [HttpGet("{id}")]
    [HasPermission(Permissions.ReadPurchaseOrder)]
    [ProducesResponseType(typeof(PurchaseOrderDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(int id, CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new GetPurchaseOrderQuery(id), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Get purchase order items by state.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/purchase-orders/1/items?status=Missing
    /// 
    /// </remarks>
    /// <param name="id">The purchase order ID</param>
    /// <param name="status">The status to filter by</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of purchase order items matching the status.</returns>
    /// <response code="200">Returns the matching items</response>
    /// <response code="404">If the purchase order is not found</response>
    [HttpGet("{id}/items")]
    [HasPermission(Permissions.ReadPurchaseOrder)]
    [ProducesResponseType(typeof(List<PurchaseOrderItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetItemsByState(int id, [FromQuery] PurchaseOrderItemStatus status, CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new GetPurchaseOrderItemsByStateQuery(id, status), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>Create Purchase Order</summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/purchase-orders
    ///     {
    ///        "supplierId": 1,
    ///        "items": [
    ///           { "productId": 1, "quantity": 10, "unitCost": 150.0 }
    ///        ]
    ///     }
    /// 
    /// </remarks>
    /// <param name="request">The purchase order creation payload</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created purchase order summary</returns>
    /// <response code="200">Returns the created purchase order summary</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="404">If the supplier or a product is not found</response>
    [HttpPost]
    [Idempotent]
    [HasPermission(Permissions.ManagePurchaseOrder)]
    [ProducesResponseType(typeof(PurchaseOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] PurchaseOrderRequest request, CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new CreatePurchaseOrderCommand(request), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>Update Purchase Order</summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     PUT /api/purchase-orders/1
    ///     {
    ///        "expectedTotalAmount": 2000.0,
    ///        "addedItems": [
    ///           { "productId": 2, "quantity": 5 }
    ///        ]
    ///     }
    /// 
    /// </remarks>
    /// <param name="id">The purchase order ID to update</param>
    /// <param name="request">The update payload</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated purchase order summary</returns>
    /// <response code="200">Returns the updated purchase order summary</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="404">If the purchase order is not found</response>
    /// <response code="409">If the purchase order is not pending</response>
    [HttpPut("{id}")]
    [HasPermission(Permissions.ManagePurchaseOrder)]
    [ProducesResponseType(typeof(PurchaseOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePurchaseOrderRequest request, CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new UpdatePurchaseOrderCommand(id, request), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>Approve / Receive Purchase Order</summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     PUT /api/purchase-orders/1/approve
    ///     {
    ///        "items": [
    ///           { "productId": 1, "receivedQuantity": 8, "unitCost": 150.0, "isMissing": false }
    ///        ]
    ///     }
    /// 
    /// </remarks>
    /// <param name="id">The purchase order ID to approve</param>
    /// <param name="request">Final quantities, costs, and missing status</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    /// <response code="204">If the purchase order was successfully approved</response>
    /// <response code="400">If a received quantity exceeds the ordered quantity</response>
    /// <response code="404">If the purchase order or an item is not found</response>
    /// <response code="409">If the purchase order is already received</response>
    [HttpPut("{id}/approve")]
    [Idempotent]
    [HasPermission(Permissions.ManagePurchaseOrder)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Approve(int id, [FromBody] ApprovePurchaseOrderRequest request, CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new ApprovePurchaseOrderCommand(id, request), cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    /// <summary>Return Purchase Order Items</summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     PUT /api/purchase-orders/1/return
    ///     {
    ///        "items": [
    ///           { "productId": 1, "quantityToReturn": 2, "returnReason": "Expired" }
    ///        ]
    ///     }
    /// 
    /// </remarks>
    /// <param name="id">The purchase order ID</param>
    /// <param name="request">Items to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    /// <response code="204">If the items were successfully returned</response>
    /// <response code="400">If the return quantity is invalid</response>
    /// <response code="404">If the purchase order or item is not found</response>
    [HttpPut("{id}/return")]
    [HasPermission(Permissions.ManagePurchaseOrder)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReturnItems(int id, [FromBody] ReturnPurchaseOrderItemsRequest request, CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new ReturnPurchaseOrderItemsCommand(id, request), cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    /// <summary>Delete Purchase Order</summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     DELETE /api/purchase-orders/1
    /// 
    /// </remarks>
    /// <param name="id">The purchase order ID to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    /// <response code="204">If the purchase order was successfully deleted</response>
    /// <response code="404">If the purchase order is not found</response>
    /// <response code="409">If the purchase order is already received and cannot be deleted</response>
    [HttpDelete("{id}")]
    [HasPermission(Permissions.ManagePurchaseOrder)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new DeletePurchaseOrderCommand(id), cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }
}
