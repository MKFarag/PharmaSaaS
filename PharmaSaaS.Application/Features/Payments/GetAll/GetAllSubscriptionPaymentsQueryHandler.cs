namespace PharmaSaaS.Application.Features.Payments.GetAll;

public sealed class GetAllSubscriptionPaymentsQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetAllSubscriptionPaymentsQuery, IEnumerable<SubscriptionPaymentResponse>>
{
    public async Task<IEnumerable<SubscriptionPaymentResponse>> Handle(GetAllSubscriptionPaymentsQuery request, CancellationToken cancellationToken = default)
    {
        var payments = await unitOfWork.Pharmacies.GetPendingPaymentsAsync(cancellationToken);

        return payments.Select(p => new SubscriptionPaymentResponse(
            p.Id,
            p.PharmacyId,
            p.Pharmacy?.Name ?? string.Empty,
            p.ImageUrl,
            p.IsVerified,
            p.Amount,
            p.SubmittedAt
        ));
    }
}
