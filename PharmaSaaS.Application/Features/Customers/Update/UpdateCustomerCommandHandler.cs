namespace PharmaSaaS.Application.Features.Customers.Update;

public sealed class UpdateCustomerCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<UpdateCustomerCommand, Result>
{
    public async Task<Result> Handle(UpdateCustomerCommand command, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.Customers.GetAsync([command.Id], cancellationToken) is not { } customer)
            return Result.Failure(CustomerErrors.NotFound);

        if (customer.PhoneNumber != command.Request.PhoneNumber &&
            await unitOfWork.Customers.AnyAsync(c => c.PhoneNumber == command.Request.PhoneNumber, cancellationToken))
            return Result.Failure(CustomerErrors.DuplicatedPhoneNumber);

        customer.Update(command.Request.FullName, command.Request.PhoneNumber);

        await unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
