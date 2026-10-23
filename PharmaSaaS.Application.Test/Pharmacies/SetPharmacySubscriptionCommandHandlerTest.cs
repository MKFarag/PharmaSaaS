using PharmaSaaS.Application.Features.Pharmacies.SetSubscription;

namespace PharmaSaaS.Application.Test.Pharmacies;

public class SetPharmacySubscriptionCommandHandlerTest
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IPharmacyRepository _pharmacyRepo = A.Fake<IPharmacyRepository>();
    private readonly SetPharmacySubscriptionCommandHandler _handler;

    public SetPharmacySubscriptionCommandHandlerTest()
    {
        A.CallTo(() => _unitOfWork.Pharmacies).Returns(_pharmacyRepo);
        _handler = new SetPharmacySubscriptionCommandHandler(_unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenPharmacyNotFound_ReturnsNotFoundError()
    {
        // Arrange
        A.CallTo(() => _pharmacyRepo.GetAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns((Pharmacy?)null);

        var command = new SetPharmacySubscriptionCommand(Guid.NewGuid().ToString(), new SetSubscriptionRequest(DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1))));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(PharmacyErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Handle_ValidData_SetsSubscriptionEndAndReturnsSuccess()
    {
        // Arrange
        var pharmacy = Pharmacy.Create("Pharmacy", "Owner", "01000000000", "owner@email.com", DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)));
        var end = new DateOnly(2027, 1, 1);

        A.CallTo(() => _pharmacyRepo.GetAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns(pharmacy);

        A.CallTo(() => _unitOfWork.SharedCompleteAsync(A<CancellationToken>.Ignored))
            .Returns(1);

        var command = new SetPharmacySubscriptionCommand(pharmacy.Id, new SetSubscriptionRequest(end));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(end, pharmacy.SubscriptionEnd);

        A.CallTo(() => _unitOfWork.SharedCompleteAsync(A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();
    }
}
