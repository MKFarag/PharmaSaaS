namespace PharmaSaaS.Application.Interfaces.Infrastructure;

public interface ISignInService
{
    Task<Result<User>> PasswordSignInAsync(string email, string password, bool lockoutOnFailure);
}
