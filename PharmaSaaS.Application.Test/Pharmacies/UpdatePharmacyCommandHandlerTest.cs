using PharmaSaaS.Application.Contracts.Pharmacies;
using PharmaSaaS.Application.Features.Pharmacies.Update;

namespace PharmaSaaS.Application.Test.Pharmacies;

public class UpdatePharmacyCommandHandlerTest
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IPharmacyRepository _pharmacyRepo = A.Fake<IPharmacyRepository>();
    private readonly UpdatePharmacyCommandHandler _handler;

    public UpdatePharmacyCommandHandlerTest()
    {
        A.CallTo(() => _unitOfWork.Pharmacies).Returns(_pharmacyRepo);
        _handler = new UpdatePharmacyCommandHandler(_unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenPharmacyNotFound_ReturnsNotFoundError()
    {
        // Arrange
        A.CallTo(() => _pharmacyRepo.GetAsync(A<int>.Ignored, A<CancellationToken>.Ignored))
            .Returns((Pharmacy?)null);

        var command = new UpdatePharmacyCommand(1, new PharmacyRequest("New Name"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(PharmacyErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Handle_WhenNameDuplicated_ReturnsDuplicatedNameError()
    {
        // Arrange
        var pharmacy = Pharmacy.Create("Old Name");
        pharmacy.Id = 1;
        pharmacy.TenantId = "tenant-1";

        A.CallTo(() => _pharmacyRepo.GetAsync(A<int>.Ignored, A<CancellationToken>.Ignored))
            .Returns(pharmacy);

        A.CallTo(() => _pharmacyRepo.NameExistsAsync(A<string>.Ignored, A<string>.Ignored, A<int>.Ignored, A<CancellationToken>.Ignored))
            .Returns(true);

        var command = new UpdatePharmacyCommand(1, new PharmacyRequest("Existing Name"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(PharmacyErrors.DuplicatedName, result.Error);
    }

    [Fact]
    public async Task Handle_ValidData_ReturnsSuccess()
    {
        // Arrange
        var pharmacy = Pharmacy.Create("Old Name");
        pharmacy.Id = 1;
        pharmacy.TenantId = "tenant-1";

        A.CallTo(() => _pharmacyRepo.GetAsync(A<int>.Ignored, A<CancellationToken>.Ignored))
            .Returns(pharmacy);

        A.CallTo(() => _pharmacyRepo.NameExistsAsync(A<string>.Ignored, A<string>.Ignored, A<int>.Ignored, A<CancellationToken>.Ignored))
            .Returns(false);

        A.CallTo(() => _unitOfWork.SharedCompleteAsync(A<CancellationToken>.Ignored))
            .Returns(1);

        var command = new UpdatePharmacyCommand(1, new PharmacyRequest("New Name"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        A.CallTo(() => _unitOfWork.SharedCompleteAsync(A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();
    }
}
