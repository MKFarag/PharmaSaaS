namespace PharmaSaaS.Application.Contracts.Products;

public record ProductResponse(
    int Id,
    string Name,
    string? Barcode,
    string? Concentration,
    decimal? MaxStorageTemperature,
    CategoryResponse Category,
    DosageFormResponse DosageForm
);
