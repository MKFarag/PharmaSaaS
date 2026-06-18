namespace PharmaSaaS.Infrastructure.Extensions;

internal static class GlobalExtensions
{
    extension(IdentityResult result)
    {
        public Result ToDomain()
        {
            if (result.Succeeded)
                return Result.Success();

            var error = result.Errors.First();
            return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
        }
    }

    extension(User user)
    {
        public ApplicationUser CreateIdentity()
            => new()
            {
                UserName = user.Email,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsDisabled = user.IsDisabled
            };
    }

    extension(Role role)
    {
        public ApplicationRole CreateIdentity()
            => new()
            {
                Name = role.Name,
                IsDisabled = role.IsDisabled,
                IsDefault = role.IsDefault
            };
    }
}
