using PharmaSaaS.Application.Features.Pharmacies.ToggleStatus;

namespace PharmaSaaS.Application.Test.Pharmacies;

public class TogglePharmacyStatusCommandHandlerTest
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IPharmacyRepository _pharmacyRepo = A.Fake<IPharmacyRepository>();
    private readonly TogglePharmacyStatusCommandHandler _handler;

    public TogglePharmacyStatusCommandHandlerTest()
    {
        A.CallTo(() => _unitOfWork.Pharmacies).Returns(_pharmacyRepo);
        _handler = new TogglePharmacyStatusCommandHandler(_unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenPharmacyNotFound_ReturnsNotFoundError()
    {
        // Arrange
        A.CallTo(() => _pharmacyRepo.GetAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns((Pharmacy?)null);

        var command = new TogglePharmacyStatusCommand(Guid.NewGuid().ToString());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(PharmacyErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Handle_WhenPharmacyIsActive_TogglesToInactive()
    {
        // Arrange
        var pharmacy = Pharmacy.Create("Pharmacy", "Owner", "01000000000", "owner@email.com", DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)));

        A.CallTo(() => _pharmacyRepo.GetAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns(pharmacy);

        A.CallTo(() => _unitOfWork.SharedCompleteAsync(A<CancellationToken>.Ignored))
            .Returns(1);

        var command = new TogglePharmacyStatusCommand(pharmacy.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(pharmacy.IsActive);

        A.CallTo(() => _unitOfWork.SharedCompleteAsync(A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenPharmacyIsNotActive_TogglesToActive()
    {
        // Arrange
        var pharmacy = Pharmacy.Create("Pharmacy", "Owner", "01000000000", "owner@email.com", DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)));
        pharmacy.ToggleStatus();

        A.CallTo(() => _pharmacyRepo.GetAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns(pharmacy);

        A.CallTo(() => _unitOfWork.SharedCompleteAsync(A<CancellationToken>.Ignored))
            .Returns(1);

        var command = new TogglePharmacyStatusCommand(pharmacy.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(pharmacy.IsActive);

        A.CallTo(() => _unitOfWork.SharedCompleteAsync(A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Deactivate_WhenActive_SetsIsActiveToFalse()
    {
        // Arrange
        var pharmacy = Pharmacy.Create("Pharmacy", "Owner", "01000000000", "owner@email.com", DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)));

        // Act
        pharmacy.Deactivate();

        // Assert
        Assert.False(pharmacy.IsActive);
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_RemainsInactive()
    {
        // Arrange
        var pharmacy = Pharmacy.Create("Pharmacy", "Owner", "01000000000", "owner@email.com", DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)));
        pharmacy.Deactivate();

        // Act
        pharmacy.Deactivate();

        // Assert
        Assert.False(pharmacy.IsActive);
    }
}
