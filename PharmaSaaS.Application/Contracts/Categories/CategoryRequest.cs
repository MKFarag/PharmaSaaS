namespace PharmaSaaS.Application.Contracts.Categories;

public record CategoryRequest(
    string Name
);

#region Validation

public class CategoryRequestValidator : AbstractValidator<CategoryRequest>
{
    public CategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .Matches(RegexPatterns.BusinessName)
            .WithMessage("Category name must start and end with a letter or number, and can contain spaces, hyphens, apostrophes, periods, underscores, and ampersands.")
            .Length(2, 100);
    }
}

#endregion
