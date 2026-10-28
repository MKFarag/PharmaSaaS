namespace PharmaSaaS.Application.Features.Payments.Upload;

public sealed class UploadSubscriptionPaymentCommandHandler(
    IUnitOfWork unitOfWork,
    IFileStorageService fileStorageService,
    INotificationService notificationService) : IRequestHandler<UploadSubscriptionPaymentCommand, Result>
{
    public async Task<Result> Handle(UploadSubscriptionPaymentCommand command, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.Pharmacies.GetAsync(command.PharmacyId, cancellationToken) is not { } pharmacy)
            return Result.Failure(PharmacyErrors.NotFound);

        var extension = Path.GetExtension(command.Image.FileName);
        var fileName = $"payment-{pharmacy.Id}-{Guid.CreateVersion7():N}{extension}";

        var relativePath = await fileStorageService.SaveAsync(command.Image.Stream, fileName, cancellationToken);

        var payment = SubscriptionPayment.Create(pharmacy.Id, relativePath);

        await unitOfWork.Pharmacies.AddPaymentAsync(payment, cancellationToken);
        await unitOfWork.SharedCompleteAsync(cancellationToken);

        await notificationService.SendSubscriptionPaymentNotificationAsync(
            pharmacy.Name,
            pharmacy.OwnerName,
            pharmacy.PhoneNumber,
            pharmacy.Email,
            payment.ImageUrl,
            payment.SubmittedAt
        );

        return Result.Success();
    }
}
