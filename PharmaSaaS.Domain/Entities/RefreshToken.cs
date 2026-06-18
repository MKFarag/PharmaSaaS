using System.Security.Cryptography;

namespace PharmaSaaS.Domain.Entities;

public class RefreshToken
{
    private const int _expiryDays = 14;
    public static int AutoRemoveAfterDays => 7;

    public string Token { get; private set; } = string.Empty;
    public DateTime ExpiresOn { get; private set; }
    public DateTime CreatedOn { get; private set; }
    public DateTime? RevokedOn { get; private set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresOn;
    public bool IsActive => RevokedOn is null && !IsExpired;

    public static RefreshToken Create() => new()
    {
        Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
        ExpiresOn = DateTime.UtcNow.AddDays(_expiryDays),
        CreatedOn = DateTime.UtcNow
    };

    public void Revoke() => RevokedOn = DateTime.UtcNow;
}
