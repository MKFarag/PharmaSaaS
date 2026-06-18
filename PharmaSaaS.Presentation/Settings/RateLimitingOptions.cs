using System.ComponentModel.DataAnnotations;

namespace PharmaSaaS.Presentation.Settings;

public sealed class RateLimitingOptions
{
    public partial class PolicyNames
    {
        public const string IpLimit = "ipLimit";
        public const string UserLimit = "userLimit";
        public const string Sliding = "sliding";
        public const string Concurrency = "Concurrency";
    }

    public PolicyOptions IpPolicy { get; init; } = default!;
    public PolicyOptions UserPolicy { get; init; } = default!;
    public ConcurrencyOptions Concurrency { get; init; } = default!;
    public SlidingWindowOptions SlidingWindow { get; init; } = default!;
}

#region RateLimiters

public class PolicyOptions
{
    [Required]
    public int PermitLimit { get; init; }

    [Required]
    public int WindowInSeconds { get; init; }

    public int QueueLimit { get; init; }
}

public class ConcurrencyOptions
{
    public int PermitLimit { get; init; }
    public int QueueLimit { get; init; }
}

public class SlidingWindowOptions : PolicyOptions
{
    public int SegmentsPerWindow { get; init; }
}

#endregion
