namespace PharmaSaaS.Domain.Entities;

public sealed class Product
{
    public int Id { get; set; }
    public string Name { get; private set; } = string.Empty;
    public string? Barcode { get; private set; }
    public string? Concentration { get; private set; }
    public decimal? MaxStorageTemperature { get; private set; }
    public int CategoryId { get; private set; }
    public int DosageFormId { get; private set; }

    public Category Category { get; private set; } = default!;
    public DosageForm DosageForm { get; private set; } = default!;
    public Inventory? Inventory { get; private set; }

    public static Product Create(string name, int categoryId, int dosageFormId, string? barcode = null, string? concentration = null, decimal? maxStorageTemperature = null)
        => new()
        {
            Name = name,
            CategoryId = categoryId,
            DosageFormId = dosageFormId,
            Barcode = barcode,
            Concentration = concentration,
            MaxStorageTemperature = maxStorageTemperature
        };

    public void Update(string name, int categoryId, int dosageFormId, string? barcode = null, string? concentration = null, decimal? maxStorageTemperature = null)
    {
        Name = name;
        CategoryId = categoryId;
        DosageFormId = dosageFormId;
        Barcode = barcode;
        Concentration = concentration;
        MaxStorageTemperature = maxStorageTemperature;
    }
    public void SetCategory(Category category) => Category = category;
    public void SetDosageForm(DosageForm dosageForm) => DosageForm = dosageForm;
}

