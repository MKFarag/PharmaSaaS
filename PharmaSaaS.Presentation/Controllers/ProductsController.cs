using PharmaSaaS.Application.Contracts.Products;
using PharmaSaaS.Application.Features.Products.Add;
using PharmaSaaS.Application.Features.Products.Delete;
using PharmaSaaS.Application.Features.Products.Get;
using PharmaSaaS.Application.Features.Products.GetAll;
using PharmaSaaS.Application.Features.Products.GetByBarcode;
using PharmaSaaS.Application.Features.Products.Update;

namespace PharmaSaaS.Presentation.Controllers;

/// <summary>
/// Manage pharmacy products.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
[EnableRateLimiting(RateLimitingOptions.PolicyNames.Concurrency)]
public class ProductsController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Retrieves a paginated list of products.
    /// </summary>
    /// <remarks>
    /// Returns a paginated list of products in the pharmacy, with optional filtering by category or dosage form,
    /// search by name, and sorting by name ascending.
    /// </remarks>
    /// <param name="filters">Pagination, search, and filter parameters (category/dosage form).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated list of products.</returns>
    /// <response code="200">Returns the paginated list of products.</response>
    /// <response code="401">If the user is unauthorized.</response>
    [HttpGet("")]
    [HasPermission(Permissions.ReadProduct)]
    [ProducesResponseType(typeof(IPaginatedList<ProductResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll([FromQuery] ProductRequestFilters filters, CancellationToken cancellationToken)
        => Ok(await sender.Send(new GetAllProductsQuery(filters), cancellationToken));

    /// <summary>
    /// Retrieves a specific product by ID.
    /// </summary>
    /// <remarks>
    /// Retrieves the details of a specific product based on its unique identifier.
    /// </remarks>
    /// <param name="id">The unique identifier of the product.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The product details.</returns>
    /// <response code="200">Returns the product details.</response>
    /// <response code="404">If the product is not found.</response>
    /// <response code="401">If the user is unauthorized.</response>
    [HttpGet("{id}")]
    [HasPermission(Permissions.ReadProduct)]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Get([FromRoute] int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetProductQuery(id), cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Looks up a product by barcode.
    /// </summary>
    /// <remarks>
    /// Finds a product by scanning its barcode. Returns the product details including category and dosage form information.
    /// </remarks>
    /// <param name="barcode">The barcode to search for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The product details.</returns>
    /// <response code="200">Returns the product details.</response>
    /// <response code="404">If no product with the specified barcode is found.</response>
    /// <response code="401">If the user is unauthorized.</response>
    [HttpGet("barcode/{barcode}")]
    [HasPermission(Permissions.ReadProduct)]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetByBarcode([FromRoute] string barcode, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetProductByBarcodeQuery(barcode), cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <remarks>
    /// Adds a new product to the pharmacy catalog.
    ///
    /// Sample request:
    ///
    ///     POST /api/Products
    ///     {
    ///       "name": "Amoxicillin 500mg",
    ///       "barcode": "6291040100216",
    ///       "concentration": "500 mg",
    ///       "maxStorageTemperature": 25.0,
    ///       "categoryId": 1,
    ///       "dosageFormId": 1
    ///     }
    ///
    /// </remarks>
    /// <param name="request">The product creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The newly created product.</returns>
    /// <response code="201">Returns the newly created product.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="401">If the user is unauthorized.</response>
    /// <response code="404">If the specified category or dosage form does not exist.</response>
    /// <response code="409">If a product with the same barcode or composite key already exists.</response>
    [HttpPost("")]
    [Idempotent]
    [HasPermission(Permissions.AddProduct)]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Add([FromBody] ProductRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new AddProductCommand(request), cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(Get), new { result.Value.Id }, result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    /// <remarks>
    /// Updates the details of an existing product by its ID.
    ///
    /// Sample request:
    ///
    ///     PUT /api/Products/1
    ///     {
    ///       "name": "Amoxicillin 1g",
    ///       "barcode": "6291040100217",
    ///       "concentration": "1 g",
    ///       "maxStorageTemperature": 25.0,
    ///       "categoryId": 1,
    ///       "dosageFormId": 1
    ///     }
    ///
    /// </remarks>
    /// <param name="id">The unique identifier of the product to update.</param>
    /// <param name="request">The updated product data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">If the product was successfully updated.</response>
    /// <response code="400">If the request data is invalid.</response>
    /// <response code="404">If the product or related category/dosage form is not found.</response>
    /// <response code="401">If the user is unauthorized.</response>
    /// <response code="409">If a product with the same barcode or composite key already exists.</response>
    [HttpPut("{id}")]
    [HasPermission(Permissions.UpdateProduct)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] ProductRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateProductCommand(id, request), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.ToProblem();
    }

    /// <summary>
    /// Deletes a product.
    /// </summary>
    /// <remarks>
    /// Deletes a product by its ID. Products with associated inventory records cannot be deleted.
    /// </remarks>
    /// <param name="id">The unique identifier of the product to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">If the product was successfully deleted.</response>
    /// <response code="404">If the product is not found.</response>
    /// <response code="401">If the user is unauthorized.</response>
    /// <response code="409">If the product has associated inventory records.</response>
    [HttpDelete("{id}")]
    [HasPermission(Permissions.DeleteProduct)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteProductCommand(id), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.ToProblem();
    }
}
