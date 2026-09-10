namespace PharmaSaaS.Application.Features.Categories.Delete;

public sealed class DeleteCategoryCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService, ICurrentTenantGetter currentTenantGetter) : IRequestHandler<DeleteCategoryCommand, Result>
{
    public async Task<Result> Handle(DeleteCategoryCommand command, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.Categories.GetAsync([command.Id], cancellationToken) is not { } category)
            return Result.Failure(CategoryErrors.NotFound);

        if (await unitOfWork.Products.AnyAsync(p => p.CategoryId == command.Id, cancellationToken))
            return Result.Failure(CategoryErrors.IsUsed);

        unitOfWork.Categories.Delete(category);

        await cacheService.RemoveByTagAsync(Cache.Tags.Categories(currentTenantGetter.GetTenantId()!), cancellationToken);

        await unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
