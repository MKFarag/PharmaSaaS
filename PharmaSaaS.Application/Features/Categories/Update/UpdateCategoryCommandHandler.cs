namespace PharmaSaaS.Application.Features.Categories.Update;

public class UpdateCategoryCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService, ICurrentTenantGetter currentTenantGetter) : IRequestHandler<UpdateCategoryCommand, Result>
{
    public async Task<Result> Handle(UpdateCategoryCommand command, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.Categories.GetAsync([command.Id], cancellationToken) is not { } category)
            return Result.Failure(CategoryErrors.NotFound);

        if (await unitOfWork.Categories.AnyAsync(c => c.Name == command.Request.Name && c.Id != command.Id, cancellationToken))
            return Result.Failure(CategoryErrors.DuplicatedName);

        category.Update(command.Request.Name);

        await unitOfWork.CompleteAsync(cancellationToken);

        await cacheService.RemoveByTagAsync(Cache.Tags.Categories(currentTenantGetter.GetTenantId()!), cancellationToken);

        return Result.Success();
    }
}
