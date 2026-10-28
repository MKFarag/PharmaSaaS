namespace PharmaSaaS.Application.Features.Payments.Verify;

public record VerifySubscriptionPaymentCommand(int PaymentId, VerifySubscriptionPaymentRequest Request) : IRequest<Result>;
