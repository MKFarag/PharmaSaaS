namespace PharmaSaaS.Domain.Entities;

public sealed class Inventory
{
    public int Id { get; set; }
    public float Quantity { get; private set; }
    public decimal SellingPrice { get; private set; }
    public int? TargetStock { get; private set; }
    public bool IsRare { get; private set; }
    public int ProductId { get; private set; }

    public Product Product { get; set; } = default!;


    public static Inventory Create(int productId, float quantity, decimal sellingPrice, int? targetStock = null)
        => new()
        {
            ProductId = productId,
            Quantity = quantity,
            SellingPrice = sellingPrice,
            TargetStock = targetStock
        };

    public void SetQuantity(float quantity) => Quantity = quantity;
    public void SetSellingPrice(decimal sellingPrice) => SellingPrice = sellingPrice;
    public void SetTargetStock(int? targetStock) => TargetStock = targetStock;
    public void SetRare(bool isRare) => IsRare = isRare;
    public void ToggleRare() => IsRare = !IsRare;
    public void DeductStock(float quantity) => Quantity -= quantity;
    public void AddStock(float quantity) => Quantity += quantity;
}
