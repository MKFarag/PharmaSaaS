namespace PharmaSaaS.Application.Contracts.Roles;

public record RoleRequest(
    string Name,
    IList<string> Permissions
);

#region Validation

public class RoleRequestValidator : AbstractValidator<RoleRequest>
{
    public RoleRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .Matches(RegexPatterns.BusinessName)
            .WithMessage("Role name must start and end with a letter or number, and can contain spaces, hyphens, apostrophes, periods, underscores, and ampersands.")
            .Length(2, 200);

        RuleFor(x => x.Permissions)
            .NotNull()
            .NotEmpty();

        RuleFor(x => x.Permissions)
            .Must(x => x.Distinct().Count() == x.Count)
            .WithMessage("You cannot add duplicated permissions for the same role.")
            .When(x => x.Permissions != null);
    }
}

#endregion
