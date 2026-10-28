

namespace PharmaSaaS.Application.Features.Payments.GetAll;

public record GetAllSubscriptionPaymentsQuery() : IRequest<IEnumerable<SubscriptionPaymentResponse>>;
