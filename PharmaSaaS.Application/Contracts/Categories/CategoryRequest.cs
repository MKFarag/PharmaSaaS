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
            .Length(2, 100);
    }
}

#endregion
