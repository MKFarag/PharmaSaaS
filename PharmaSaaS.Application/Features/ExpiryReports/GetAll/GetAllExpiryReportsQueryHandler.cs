namespace PharmaSaaS.Application.Features.ExpiryReports.GetAll;

public sealed class GetAllExpiryReportsQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetAllExpiryReportsQuery, IPaginatedList<ExpiryReportResponse>>
{
    public async Task<IPaginatedList<ExpiryReportResponse>> Handle(GetAllExpiryReportsQuery request, CancellationToken cancellationToken = default)
    {
        return await unitOfWork.ExpiryReports.FindPaginatedListAsync<ExpiryReportResponse>(
            er => !request.Filters.Year.HasValue || er.ReportDate.Year == request.Filters.Year.Value,
            request.Filters.PageNumber, request.Filters.PageSize,
            request.Filters.ProductName, $"{nameof(ExpiryReport.Product)}.{nameof(Product.Name)}",
            nameof(ExpiryReport.ReportDate), OrderBy.Descending,
            ColumnType.String,
            [
                nameof(ExpiryReport.Product),
                $"{nameof(ExpiryReport.Product)}.{nameof(Product.Category)}",
                $"{nameof(ExpiryReport.Product)}.{nameof(Product.DosageForm)}"
            ],
            cancellationToken);
    }
}
