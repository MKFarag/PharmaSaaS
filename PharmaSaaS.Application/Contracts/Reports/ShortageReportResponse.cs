namespace PharmaSaaS.Application.Contracts.Reports;

public record ShortageReportResponse(
    int ProductId,
    string ProductName,
    float Quantity,
    int? TargetStock,
    bool IsRare,
    string ShortageType
);
