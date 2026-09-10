namespace PharmaSaaS.Application.Contracts.Invoices;

public record CheckReturnImpactResponse(
    decimal EstimatedRefund,
    decimal DiscountClawback,
    bool WillDropBelowLoyaltyTier,
    string? CurrentTierName,
    decimal? CurrentTierThreshold,
    decimal ProjectedSpendAfterReturn
);
