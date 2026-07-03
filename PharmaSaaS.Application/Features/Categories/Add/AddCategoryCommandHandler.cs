namespace PharmaSaaS.Application.Features.Categories.Add;

public class AddCategoryCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService, ICurrentTenantGetter currentTenantGetter) : IRequestHandler<AddCategoryCommand, Result<CategoryResponse>>
{
    public async Task<Result<CategoryResponse>> Handle(AddCategoryCommand command, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.Categories.AnyAsync(c => c.Name == command.Request.Name, cancellationToken))
            return Result.Failure<CategoryResponse>(CategoryErrors.DuplicatedName);

        var category = Category.Create(command.Request.Name);

        await unitOfWork.Categories.AddAsync(category, cancellationToken);

        await unitOfWork.CompleteAsync(cancellationToken);

        await cacheService.RemoveByTagAsync(Cache.Tags.Categories(currentTenantGetter.GetTenantId()!), cancellationToken);

        return Result.Success(category.Adapt<CategoryResponse>());
    }
}
