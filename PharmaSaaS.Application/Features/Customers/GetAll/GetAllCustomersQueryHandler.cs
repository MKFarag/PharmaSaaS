namespace PharmaSaaS.Application.Features.Customers.GetAll;

public sealed class GetAllCustomersQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetAllCustomersQuery, IPaginatedList<CustomerResponse>>
{
    public async Task<IPaginatedList<CustomerResponse>> Handle(GetAllCustomersQuery request, CancellationToken cancellationToken = default)
    {
        var f = request.Filters;
        f = f.Check([nameof(Customer.FullName), nameof(Customer.TotalSpend)],
                    [nameof(Customer.FullName), nameof(Customer.PhoneNumber)]);

        return await unitOfWork.Customers.GetPaginatedListAsync<CustomerResponse>(
            f.PageNumber, f.PageSize,
            f.SearchValue, f.SearchColumn,
            f.SortColumn!, f.SortDirection!,
            ColumnType.String,
            [
                nameof(Customer.CurrentTier),
                $"{nameof(Customer.CurrentTier)}.{nameof(LoyaltyTier.DiscountRules)}"
            ],
            cancellationToken);
    }
}
