namespace PharmaSaaS.Application.Features.Customers.Get;

public sealed class GetCustomerByIdQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetCustomerByIdQuery, Result<CustomerResponse>>
{
    public async Task<Result<CustomerResponse>> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken = default)
    {
        var customer = await unitOfWork.Customers.FindAsync(
            c => c.Id == request.Id,
            [nameof(Customer.CurrentTier), $"{nameof(Customer.CurrentTier)}.{nameof(LoyaltyTier.DiscountRules)}"],
            cancellationToken);

        return customer is null
            ? Result.Failure<CustomerResponse>(CustomerErrors.NotFound)
            : Result.Success(customer.Adapt<CustomerResponse>());
    }
}
