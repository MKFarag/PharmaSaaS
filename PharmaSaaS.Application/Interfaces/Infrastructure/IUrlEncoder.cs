namespace PharmaSaaS.Application.Interfaces.Infrastructure;

public interface IUrlEncoder
{
    string Encode(string value);
    string? Decode(string encodedValue);
}
