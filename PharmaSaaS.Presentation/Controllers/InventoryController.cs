using PharmaSaaS.Application.Contracts.Inventories;
using PharmaSaaS.Application.Features.Inventories.AdjustStock;
using PharmaSaaS.Application.Features.Inventories.GetAllInventories;
using PharmaSaaS.Application.Features.Inventories.ToggleRare;
using PharmaSaaS.Application.Features.Inventories.UpdateStock;

namespace PharmaSaaS.Presentation.Controllers;

/// <summary>
/// Manage product inventory and stock levels.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
[EnableRateLimiting(RateLimitingOptions.PolicyNames.Concurrency)]
public class InventoryController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Retrieves a paginated list of inventory records.
    /// </summary>
    /// <remarks>
    /// Returns stock levels for all products with pagination and optional filtering.
    /// All filters are optional and combinable.
    ///
    /// Sample requests:
    ///
    ///     GET /api/Inventory?pageNumber=1&amp;pageSize=10
    ///     GET /api/Inventory?searchValue=panadol
    ///     GET /api/Inventory?isRare=true
    ///     GET /api/Inventory?belowTarget=true
    ///     GET /api/Inventory?outOfStock=true
    ///
    /// </remarks>
    /// <param name="filters">Pagination, search, and inventory filters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated list of inventory records.</returns>
    /// <response code="200">Returns the paginated inventory list.</response>
    /// <response code="401">If the user is unauthorized.</response>
    [HttpGet("")]
    [HasPermission(Permissions.ReadInventory)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll([FromQuery] InventoryRequestFilters filters, CancellationToken cancellationToken)
        => Ok(await sender.Send(new GetAllInventoriesQuery(filters), cancellationToken));

    /// <summary>
    /// Updates stock for a specific product.
    /// </summary>
    /// <param name="productId">The product identifier.</param>
    /// <param name="request">The stock update request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">If the stock was successfully updated.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="404">If the inventory record is not found.</response>
    /// <response code="401">If the user is unauthorized.</response>
    [HttpPut("{productId}")]
    [HasPermission(Permissions.UpdateInventory)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateStock([FromRoute] int productId, [FromBody] UpdateStockRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateStockCommand(productId, request), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.ToProblem();
    }

    /// <summary>
    /// Toggles the rare-in-market flag for a product.
    /// </summary>
    /// <remarks>
    /// Manually marks a product as rare (scarce in the market) regardless of stock quantity.
    /// Calling this again removes the flag.
    /// </remarks>
    /// <param name="productId">The product identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">If the flag was toggled successfully.</response>
    /// <response code="404">If the inventory record is not found.</response>
    /// <response code="401">If the user is unauthorized.</response>
    [HttpPatch("{productId}/rare")]
    [HasPermission(Permissions.ToggleRare)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ToggleRare([FromRoute] int productId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ToggleRareCommand(productId), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.ToProblem();
    }

    /// <summary>
    /// Adjusts the stock quantity for a specific product.
    /// </summary>
    /// <param name="productId">The product identifier.</param>
    /// <param name="request">The stock adjustment request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">If the stock was successfully adjusted.</response>
    /// <response code="400">If the request is invalid or adjustment results in negative stock.</response>
    /// <response code="404">If the inventory record is not found.</response>
    /// <response code="401">If the user is unauthorized.</response>
    [HttpPatch("{productId}/stock")]
    [HasPermission(Permissions.AdjustInventoryStock)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AdjustStock([FromRoute] int productId, [FromBody] AdjustStockRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new AdjustStockCommand(productId, request), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.ToProblem();
    }
}
