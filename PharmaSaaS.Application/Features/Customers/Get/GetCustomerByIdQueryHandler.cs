namespace PharmaSaaS.Application.Features.Customers.Get;

public sealed class GetCustomerByIdQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetCustomerByIdQuery, Result<CustomerResponse>>
{
    public async Task<Result<CustomerResponse>> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.Customers.FindAsync(c => c.Id == request.Id, [nameof(Customer.CurrentTier)], cancellationToken) is not { } customer)
            return Result.Failure<CustomerResponse>(CustomerErrors.NotFound);

        return Result.Success(customer.Adapt<CustomerResponse>());
    }
}
