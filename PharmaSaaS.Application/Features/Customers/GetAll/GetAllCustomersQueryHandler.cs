namespace PharmaSaaS.Application.Features.Customers.GetAll;

public sealed class GetAllCustomersQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetAllCustomersQuery, IPaginatedList<CustomerResponse>>
{
    public async Task<IPaginatedList<CustomerResponse>> Handle(GetAllCustomersQuery request, CancellationToken cancellationToken = default)
    {
        request.Filters.Check([nameof(Customer.FullName), nameof(Customer.TotalSpend)],
                              [nameof(Customer.FullName), nameof(Customer.PhoneNumber)]);

        return await unitOfWork.Customers.GetPaginatedListAsync<CustomerResponse>(
            request.Filters.PageNumber, request.Filters.PageSize,
            request.Filters.SearchValue, request.Filters.SearchColumn,
            request.Filters.SortColumn!, request.Filters.SortDirection!,
            ColumnType.String,
            [
                nameof(Customer.CurrentTier),
                $"{nameof(Customer.CurrentTier)}.{nameof(LoyaltyTier.DiscountRules)}"
            ],
            cancellationToken);
    }
}
