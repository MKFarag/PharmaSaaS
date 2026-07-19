namespace PharmaSaaS.Application.Features.Categories.Get;

public class GetCategoryQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetCategoryQuery, Result<CategoryResponse>>
{
    public async Task<Result<CategoryResponse>> Handle(GetCategoryQuery request, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.Categories.GetAsync([request.Id], cancellationToken) is not { } category)
            return Result.Failure<CategoryResponse>(CategoryErrors.NotFound);

        return Result.Success(category.Adapt<CategoryResponse>());
    }
}