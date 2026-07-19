namespace PharmaSaaS.Application.Features.ExpiryReports.GetAll;

public sealed class GetAllExpiryReportsQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetAllExpiryReportsQuery, IPaginatedList<ExpiryReportResponse>>
{
    public async Task<IPaginatedList<ExpiryReportResponse>> Handle(GetAllExpiryReportsQuery request, CancellationToken cancellationToken = default)
    {
        var f = request.Filters;

        return await unitOfWork.ExpiryReports.FindPaginatedListAsync<ExpiryReportResponse>(
            er => !f.Year.HasValue || er.ReportDate.Year == f.Year.Value,
            f.PageNumber, f.PageSize,
            f.SearchValue, $"{nameof(ExpiryReport.Product)}.{nameof(Product.Name)}",
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
