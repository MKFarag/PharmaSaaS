namespace PharmaSaaS.Application.Contracts.Categories;

public record CategoryRequest(
    string Name
);

public class CategoryRequestValidator : AbstractValidator<CategoryRequest>
{
    public CategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .Length(2, 100);
    }
}
