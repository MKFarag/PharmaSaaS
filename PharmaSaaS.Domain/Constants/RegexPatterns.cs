namespace PharmaSaaS.Domain.Constants;

public static class RegexPatterns
{
    // Complex password pattern
    public const string Password = "(?=(.*[0-9]))(?=.*[\\!@#$%^&*()\\\\[\\]{}\\-_+=~`|:;\"'<>,./?])(?=.*[a-z])(?=(.*[A-Z]))(?=(.*)).{8,}";

    // Only contain letters, numbers, and underscores
    public const string AlphanumericUnderscore = @"^[a-zA-Z0-9_]+$";

    // Only contain letters and spaces in the middle.
    public const string OnlyLettersWithSpaces = @"^[A-Za-z]+(?:\s[A-Za-z]+)*$";

    // Only contain numbers
    public const string OnlyNumbers = @"^[0-9]+$";

    // Only contain letters, numbers, underscores, hyphens, and periods
    public const string SafeFileName = @"^[A-Za-z0-9_\-\.]*$";

    public const string PhoneNumber = @"^(?:(?:\+|00)[1-9]\d{6,14}|01[0125]\d{8}|0[2-9]\d{7,8})$";
}