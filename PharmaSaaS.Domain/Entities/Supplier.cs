namespace PharmaSaaS.Domain.Entities;

public sealed class Supplier
{
    public int Id { get; set; }
    public string Name { get; private set; } = string.Empty;
    public string? ContactInfo { get; private set; }
    public decimal Balance { get; private set; }
    public DateOnly? LastOrderDate { get; private set; }

    public ICollection<PurchaseOrder> PurchaseOrders { get; set; } = default!;

    public static Supplier Create(string name, string? contactInfo)
        => new()
        {
            Name = name,
            ContactInfo = contactInfo,
            Balance = 0,
            LastOrderDate = null
        };

    public void Update(string name, string? contactInfo)
    {
        Name = name;
        ContactInfo = contactInfo;
    }

    public void AddOrder(decimal totalAmount)
    {
        Balance -= totalAmount;
        LastOrderDate = DateOnly.FromDateTime(DateTime.UtcNow);
    }

    public void RemoveOrder(decimal totalAmount)
    {
        Balance += totalAmount;
    }

    public void ResetBalance() => Balance = 0;
}
