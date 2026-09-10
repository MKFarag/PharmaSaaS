namespace PharmaSaaS.Application.Features.Inventories.ToggleRare;

public sealed class ToggleRareCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<ToggleRareCommand, Result>
{
    public async Task<Result> Handle(ToggleRareCommand command, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.Inventories.TrackedFindAsync(i => i.ProductId == command.ProductId, cancellationToken) is not { } inventory)
            return Result.Failure(InventoryErrors.NotFound);

        inventory.ToggleRare();

        await unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
