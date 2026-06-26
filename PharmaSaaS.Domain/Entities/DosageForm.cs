namespace PharmaSaaS.Domain.Entities;

public sealed class DosageForm
{
    public int Id { get; set; }
    public string Name { get; private set; } = string.Empty;

    public static DosageForm Create(string name) => new() { Name = name };
    public void Update(string name) => Name = name;
}
