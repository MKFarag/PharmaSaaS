namespace PharmaSaaS.Application.Features.Reports.GetShortageReport;

public sealed class GetShortageReportQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetShortageReportQuery, IEnumerable<ShortageReportResponse>>
{
    public async Task<IEnumerable<ShortageReportResponse>> Handle(GetShortageReportQuery request, CancellationToken cancellationToken = default)
    {
        var inventories = await unitOfWork.Inventories.FindAllAsync(
            x => x.IsRare || (x.TargetStock != null && x.Quantity <= x.TargetStock * 0.25f),
            [nameof(Inventory.Product)],
            cancellationToken);

        return inventories.Select(x =>
        {
            bool isAutoShortage = x.TargetStock.HasValue && x.Quantity <= x.TargetStock.Value * 0.25f;
            string shortageType = x.IsRare && isAutoShortage ? "Both" : x.IsRare ? "Manual" : "Auto";
            return new ShortageReportResponse(x.ProductId, x.Product.Name, x.Quantity, x.TargetStock, x.IsRare, shortageType);
        });
    }
}
