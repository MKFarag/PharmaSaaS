namespace PharmaSaaS.Infrastructure.Services;

public class SignInService(UserManager<ApplicationUser> userManager) : ISignInService
{
    public async Task<Result<User>> PasswordSignInAsync(string email, string password, bool lockoutOnFailure)
    {
        if (await userManager.FindByEmailAsync(email) is not { } user)
            return Result.Failure<User>(UserErrors.InvalidCredentials);

        if (lockoutOnFailure && await userManager.IsLockedOutAsync(user))
            return Result.Failure<User>(UserErrors.LockedUser);

        if (!user.EmailConfirmed)
            return Result.Failure<User>(UserErrors.EmailNotConfirmed);

        if (!await userManager.CheckPasswordAsync(user, password))
        {
            if (lockoutOnFailure)
                await userManager.AccessFailedAsync(user);

            return Result.Failure<User>(UserErrors.InvalidCredentials);
        }

        if (lockoutOnFailure && user.AccessFailedCount > 0)
            await userManager.ResetAccessFailedCountAsync(user);

        return Result.Success(user.Adapt<User>());
    }
}