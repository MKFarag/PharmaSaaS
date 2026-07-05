namespace PharmaSaaS.Application.Features.Products.GetByBarcode;

public record GetProductByBarcodeQuery(string Barcode) : IRequest<Result<ProductResponse>>;
