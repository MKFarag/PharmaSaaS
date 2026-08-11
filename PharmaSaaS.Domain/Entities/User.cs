namespace PharmaSaaS.Domain.Entities;

public sealed class User
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public bool IsDisabled { get; private set; }

    public string FullName => $"{FirstName} {LastName}";

    public static User Create(string email, string firstName, string lastName)
        => new() { Email = email, FirstName = firstName, LastName = lastName };

    public void Update(string email, string firstName, string lastName)
    {
        Email = email;
        FirstName = firstName;
        LastName = lastName;
    }

    public void ChangeName(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }

    public void ToggleStatus() => IsDisabled = !IsDisabled;
}

