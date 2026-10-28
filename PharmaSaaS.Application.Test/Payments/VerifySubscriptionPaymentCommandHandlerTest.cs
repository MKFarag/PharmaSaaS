using PharmaSaaS.Application.Contracts.Payments;
using PharmaSaaS.Application.Features.Payments.Verify;

namespace PharmaSaaS.Application.Test.Payments;

public class VerifySubscriptionPaymentCommandHandlerTest
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IPharmacyRepository _pharmacyRepo = A.Fake<IPharmacyRepository>();
    private readonly VerifySubscriptionPaymentCommandHandler _handler;

    public VerifySubscriptionPaymentCommandHandlerTest()
    {
        A.CallTo(() => _unitOfWork.Pharmacies).Returns(_pharmacyRepo);
        _handler = new VerifySubscriptionPaymentCommandHandler(_unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenPaymentNotFound_ReturnsNotFoundError()
    {
        A.CallTo(() => _pharmacyRepo.GetPaymentAsync(1, A<CancellationToken>.Ignored))
            .Returns((SubscriptionPayment?)null);

        var command = new VerifySubscriptionPaymentCommand(1, new VerifySubscriptionPaymentRequest(1, 100));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(SubscriptionPaymentErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Handle_WhenAlreadyVerified_ReturnsAlreadyVerifiedError()
    {
        var payment = SubscriptionPayment.Create("pharm-1", "/images/payments/receipt.png");
        payment.Verify(100);

        A.CallTo(() => _pharmacyRepo.GetPaymentAsync(1, A<CancellationToken>.Ignored))
            .Returns(payment);

        var command = new VerifySubscriptionPaymentCommand(1, new VerifySubscriptionPaymentRequest(1, 100));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(SubscriptionPaymentErrors.AlreadyVerified, result.Error);
    }

    [Fact]
    public async Task Handle_WhenValid_VerifiesPaymentAndExtendsSubscription()
    {
        var pharmacy = Pharmacy.Create("Test Pharmacy", "Owner", "0123456789", "owner@test.com", DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5)));
        pharmacy.Deactivate();

        var payment = SubscriptionPayment.Create(pharmacy.Id, "/images/payments/receipt.png");

        A.CallTo(() => _pharmacyRepo.GetPaymentAsync(1, A<CancellationToken>.Ignored))
            .Returns(payment);

        A.CallTo(() => _pharmacyRepo.GetAsync(pharmacy.Id, A<CancellationToken>.Ignored))
            .Returns(pharmacy);

        A.CallTo(() => _unitOfWork.SharedCompleteAsync(A<CancellationToken>.Ignored))
            .Returns(1);

        var command = new VerifySubscriptionPaymentCommand(1, new VerifySubscriptionPaymentRequest(3, 1500));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(payment.IsVerified);
        Assert.Equal(1500, payment.Amount);
        Assert.True(pharmacy.IsActive);
        Assert.True(pharmacy.SubscriptionEnd > DateOnly.FromDateTime(DateTime.UtcNow));

        A.CallTo(() => _unitOfWork.SharedCompleteAsync(A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();
    }
}
