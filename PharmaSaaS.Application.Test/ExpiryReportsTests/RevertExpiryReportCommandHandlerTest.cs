namespace PharmaSaaS.Application.Test.ExpiryReportsTests;

public class RevertExpiryReportCommandHandlerTest
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IGenericRepositoryWithPagination<ExpiryReport> _expiryReportRepo = A.Fake<IGenericRepositoryWithPagination<ExpiryReport>>();
    private readonly IGenericRepositoryWithPagination<Inventory> _inventoryRepo = A.Fake<IGenericRepositoryWithPagination<Inventory>>();
    private readonly RevertExpiryReportCommandHandler _handler;

    public RevertExpiryReportCommandHandlerTest()
    {
        A.CallTo(() => _unitOfWork.ExpiryReports).Returns(_expiryReportRepo);
        A.CallTo(() => _unitOfWork.Inventories).Returns(_inventoryRepo);
        _handler = new RevertExpiryReportCommandHandler(_unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenExpiryReportNotFound_ReturnsNotFoundError()
    {
        A.CallTo(() => _expiryReportRepo.GetAsync(
                A<object[]>.Ignored,
                A<CancellationToken>.Ignored))
            .Returns(Task.FromResult<ExpiryReport?>(null));

        var result = await _handler.Handle(new RevertExpiryReportCommand(1), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ExpiryReportErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Handle_ValidData_AddsStockAndDeletesReport()
    {
        var expiryReport = ExpiryReport.Create(1, 10f, "user-1", "notes");
        expiryReport.Id = 1;

        var inventory = Inventory.Create(1, 50f, 5.50m, 50);

        A.CallTo(() => _expiryReportRepo.GetAsync(
                A<object[]>.Ignored,
                A<CancellationToken>.Ignored))
            .Returns(Task.FromResult<ExpiryReport?>(expiryReport));

        A.CallTo(() => _inventoryRepo.TrackedFindAsync(
                A<Expression<Func<Inventory, bool>>>.Ignored,
                A<CancellationToken>.Ignored))
            .Returns(Task.FromResult<Inventory?>(inventory));

        A.CallTo(() => _unitOfWork.CompleteAsync(A<CancellationToken>.Ignored))
            .Returns(1);

        var result = await _handler.Handle(new RevertExpiryReportCommand(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(60f, inventory.Quantity);

        A.CallTo(() => _expiryReportRepo.Delete(expiryReport))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _unitOfWork.CompleteAsync(A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();
    }
}
