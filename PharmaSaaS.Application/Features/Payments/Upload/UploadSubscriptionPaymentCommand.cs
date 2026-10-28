namespace PharmaSaaS.Application.Features.Payments.Upload;

public record UploadSubscriptionPaymentCommand(string PharmacyId, FileData Image) : IRequest<Result>;
