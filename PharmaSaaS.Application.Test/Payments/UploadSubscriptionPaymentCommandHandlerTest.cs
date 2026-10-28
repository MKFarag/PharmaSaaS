using PharmaSaaS.Application.Features.Payments.Upload;

namespace PharmaSaaS.Application.Test.Payments;

public class UploadSubscriptionPaymentCommandHandlerTest
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IPharmacyRepository _pharmacyRepo = A.Fake<IPharmacyRepository>();
    private readonly IFileStorageService _fileStorageService = A.Fake<IFileStorageService>();
    private readonly INotificationService _notificationService = A.Fake<INotificationService>();
    private readonly UploadSubscriptionPaymentCommandHandler _handler;

    public UploadSubscriptionPaymentCommandHandlerTest()
    {
        A.CallTo(() => _unitOfWork.Pharmacies).Returns(_pharmacyRepo);
        _handler = new UploadSubscriptionPaymentCommandHandler(_unitOfWork, _fileStorageService, _notificationService);
    }

    [Fact]
    public async Task Handle_WhenPharmacyNotFound_ReturnsPharmacyNotFoundError()
    {
        A.CallTo(() => _pharmacyRepo.GetAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns((Pharmacy?)null);

        using var memoryStream = new MemoryStream();
        using var fileData = new FileData("receipt.png", "image/png", 1024, memoryStream);
        var command = new UploadSubscriptionPaymentCommand("invalid-pharmacy-id", fileData);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(PharmacyErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Handle_WhenValid_SavesFileAndAddsPaymentAndSendsNotification()
    {
        var pharmacy = Pharmacy.Create("Test Pharmacy", "Owner", "0123456789", "owner@test.com", DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1)));
        A.CallTo(() => _pharmacyRepo.GetAsync(pharmacy.Id, A<CancellationToken>.Ignored))
            .Returns(pharmacy);

        A.CallTo(() => _fileStorageService.SaveAsync(A<Stream>.Ignored, A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns("/images/payments/test.png");

        A.CallTo(() => _unitOfWork.SharedCompleteAsync(A<CancellationToken>.Ignored))
            .Returns(1);

        using var memoryStream = new MemoryStream();
        using var fileData = new FileData("receipt.png", "image/png", 1024, memoryStream);
        var command = new UploadSubscriptionPaymentCommand(pharmacy.Id, fileData);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);

        A.CallTo(() => _fileStorageService.SaveAsync(A<Stream>.Ignored, A<string>.Ignored, A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _pharmacyRepo.AddPaymentAsync(A<SubscriptionPayment>.Ignored, A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _unitOfWork.SharedCompleteAsync(A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _notificationService.SendSubscriptionPaymentNotificationAsync(
            pharmacy.Name,
            pharmacy.OwnerName,
            pharmacy.PhoneNumber,
            pharmacy.Email,
            A<string>.Ignored,
            A<DateTime>.Ignored))
            .MustHaveHappenedOnceExactly();
    }
}
