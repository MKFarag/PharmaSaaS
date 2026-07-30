namespace PharmaSaaS.Application.Contracts.Invoices;

/// <summary>
/// Response for the return-impact preview query.
/// </summary>
public record CheckReturnImpactResponse(
    /// <summary>The amount the customer would receive back if the return is processed.</summary>
    decimal EstimatedRefund,

    /// <summary>
    /// The portion of the original discount that would be clawed back because the remaining
    /// purchase total drops below the loyalty discount threshold. Zero if the discount is retained.
    /// </summary>
    decimal DiscountClawback,

    /// <summary>
    /// Whether processing this return would cause the customer's accumulated spend to drop
    /// below the threshold required for their current loyalty tier, resulting in a tier downgrade.
    /// </summary>
    bool WillDropBelowLoyaltyTier,

    /// <summary>The name of the customer's current loyalty tier, or null for anonymous customers.</summary>
    string? CurrentTierName,

    /// <summary>
    /// The spend threshold of the current tier. The customer will be downgraded if their
    /// new spend after the return falls below this value.
    /// </summary>
    decimal? CurrentTierThreshold,

    /// <summary>The customer's projected total spend after the return is processed.</summary>
    decimal ProjectedSpendAfterReturn
);
