namespace PharmaSaaS.Domain.Entities;

public sealed class Inventory
{
    public int Id { get; set; }
    public float Quantity { get; private set; }
    public decimal SellingPrice { get; private set; }
    public int? TargetStock { get; private set; }
    public bool IsRare { get; private set; }
    public DateOnly? LastSoldAt { get; private set; }
    public DateOnly? LastOrderedAt { get; private set; }
    public float TotalUnitsSold { get; private set; }
    public int ProductId { get; private set; }

    public Product Product { get; private set; } = default!;


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
    public bool TryAdjustStock(float amount, out float newQuantity)
    {
        newQuantity = Quantity + amount;
        if (newQuantity < 0)
            return false;
        Quantity = newQuantity;
        return true;
    }
    public bool TryDeductStock(float quantity)
    {
        if (quantity > Quantity)
            return false;
        Quantity -= quantity;
        return true;
    }
    public void SetLastSold() => LastSoldAt = DateOnly.FromDateTime(DateTime.UtcNow);
    public void SetLastOrdered() => LastOrderedAt = DateOnly.FromDateTime(DateTime.UtcNow);
    public void AddToSoldUnits(float quantity) => TotalUnitsSold += quantity;
}

