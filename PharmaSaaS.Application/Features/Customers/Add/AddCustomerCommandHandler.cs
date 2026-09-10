namespace PharmaSaaS.Application.Features.Customers.Add;

public sealed class AddCustomerCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<AddCustomerCommand, Result<CustomerResponse>>
{
    public async Task<Result<CustomerResponse>> Handle(AddCustomerCommand command, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.Customers.AnyAsync(c => c.PhoneNumber == command.Request.PhoneNumber, cancellationToken))
            return Result.Failure<CustomerResponse>(CustomerErrors.DuplicatedPhoneNumber);

        var customer = Customer.Create(command.Request.FullName, command.Request.PhoneNumber);

        await unitOfWork.Customers.AddAsync(customer, cancellationToken);

        await unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success(customer.Adapt<CustomerResponse>());
    }
}
