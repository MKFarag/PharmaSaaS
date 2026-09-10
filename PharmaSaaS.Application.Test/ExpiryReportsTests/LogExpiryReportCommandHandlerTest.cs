namespace PharmaSaaS.Application.Test.ExpiryReportsTests;

public class LogExpiryReportCommandHandlerTest
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IGenericRepositoryWithPagination<Inventory> _inventoryRepo = A.Fake<IGenericRepositoryWithPagination<Inventory>>();
    private readonly IGenericRepositoryWithPagination<ExpiryReport> _expiryReportRepo = A.Fake<IGenericRepositoryWithPagination<ExpiryReport>>();
    private readonly LogExpiryReportCommandHandler _handler;

    public LogExpiryReportCommandHandlerTest()
    {
        A.CallTo(() => _unitOfWork.Inventories).Returns(_inventoryRepo);
        A.CallTo(() => _unitOfWork.ExpiryReports).Returns(_expiryReportRepo);
        _handler = new LogExpiryReportCommandHandler(_unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenInventoryNotFound_ReturnsProductNotFoundError()
    {
        A.CallTo(() => _inventoryRepo.TrackedFindAsync(
                A<Expression<Func<Inventory, bool>>>.Ignored,
                A<string[]>.Ignored,
                A<CancellationToken>.Ignored))
            .Returns(Task.FromResult<Inventory?>(null));

        var result = await _handler.Handle(new LogExpiryReportCommand(new ExpiryReportRequest(1, 10, null), "user-1"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(InventoryErrors.ProductNotFound, result.Error);
    }

    [Fact]
    public async Task Handle_WhenQuantityExceedsStock_ReturnsQuantityExceedsStockError()
    {
        var inventory = Inventory.Create(1, 5f, 5.50m, 50);

        A.CallTo(() => _inventoryRepo.TrackedFindAsync(
                A<Expression<Func<Inventory, bool>>>.Ignored,
                A<string[]>.Ignored,
                A<CancellationToken>.Ignored))
            .Returns(Task.FromResult<Inventory?>(inventory));

        var result = await _handler.Handle(new LogExpiryReportCommand(new ExpiryReportRequest(1, 10, null), "user-1"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(InventoryErrors.QuantityExceedsStock, result.Error);
    }

    [Fact]
    public async Task Handle_ValidData_ReturnsSuccess()
    {
        var category = Category.Create("Cat");
        var dosageForm = DosageForm.Create("Form");
        var product = Product.Create("Product 1", 1, 1);
        product.SetCategory(category);
        product.SetDosageForm(dosageForm);

        var inventory = Inventory.Create(1, 50f, 5.50m, 50);

        var prop = typeof(Inventory).GetProperty("Product");
        prop?.SetValue(inventory, product);

        A.CallTo(() => _inventoryRepo.TrackedFindAsync(
                A<Expression<Func<Inventory, bool>>>.Ignored,
                A<string[]>.Ignored,
                A<CancellationToken>.Ignored))
            .Returns(Task.FromResult<Inventory?>(inventory));

        A.CallTo(() => _unitOfWork.CompleteAsync(A<CancellationToken>.Ignored))
            .Returns(1);

        var result = await _handler.Handle(new LogExpiryReportCommand(new ExpiryReportRequest(1, 10, "Expired batch"), "user-1"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(10f, result.Value.ExpiredQuantity);
        Assert.Equal(40f, inventory.Quantity);

        A.CallTo(() => _expiryReportRepo.AddAsync(A<ExpiryReport>.Ignored, A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _unitOfWork.CompleteAsync(A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();
    }
}
