using Microsoft.AspNetCore.Mvc;
using PharmaSaaS.Application.Contracts.PurchaseOrders;
using PharmaSaaS.Application.Features.PurchaseOrders.Create;
using PharmaSaaS.Application.Features.PurchaseOrders.Delete;
using PharmaSaaS.Application.Features.PurchaseOrders.Get;
using PharmaSaaS.Application.Features.PurchaseOrders.List;
using PharmaSaaS.Application.Features.PurchaseOrders.Receive;
using PharmaSaaS.Domain.Abstraction;
using PharmaSaaS.Domain.Constants;
using PharmaSaaS.Presentation.Attributes;

namespace PharmaSaaS.Presentation.Controllers;

/// <summary>Manage Purchase Orders.</summary>
[Route("api/purchase-orders")]
[ApiController]
public class PurchaseOrdersController(ISender sender) : ControllerBase
{
    /// <summary>List Purchase Orders</summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     GET /api/purchase-orders?supplierId=1
    /// 
    /// </remarks>
    /// <param name="supplierId">Optional supplier filter</param>
    /// <param name="date">Optional date filter</param>
    /// <param name="pageNumber">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A paginated list of purchase orders.</returns>
    /// <response code="200">Returns the paginated list of purchase orders</response>
    [HttpGet]
    [HasPermission(Permissions.ReadPurchaseOrder)]
    [ProducesResponseType(typeof(IPaginatedList<PurchaseOrderResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] int? supplierId,
        [FromQuery] DateOnly? date,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new ListPurchaseOrdersQuery(supplierId, date, pageNumber, pageSize), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
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
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(int id, CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new GetPurchaseOrderQuery(id), cancellationToken);
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
    [HasPermission(Permissions.ManagePurchaseOrder)]
    [ProducesResponseType(typeof(PurchaseOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] PurchaseOrderRequest request, CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new CreatePurchaseOrderCommand(request), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>Receive Purchase Order</summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     PUT /api/purchase-orders/1/receive
    /// 
    /// </remarks>
    /// <param name="id">The purchase order ID to mark as received</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    /// <response code="204">If the purchase order was successfully received</response>
    /// <response code="404">If the purchase order is not found</response>
    /// <response code="409">If the purchase order is already received</response>
    [HttpPut("{id}/receive")]
    [HasPermission(Permissions.ManagePurchaseOrder)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Receive(int id, CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new ReceivePurchaseOrderCommand(id), cancellationToken);
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
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new DeletePurchaseOrderCommand(id), cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }
}
