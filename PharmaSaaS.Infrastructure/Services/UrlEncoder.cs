using Microsoft.AspNetCore.WebUtilities;

namespace PharmaSaaS.Infrastructure.Services;

public class UrlEncoder : IUrlEncoder
{
    public string Encode(string value)
        => WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(value));

    public string? Decode(string encodedValue)
    {
        try
        {
            return Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(encodedValue));
        }
        catch (FormatException)
        {
            return null;
        }
    }
}