using PharmaSaaS.Application.Features.Payments.GetAll;

namespace PharmaSaaS.Application.Test.Payments;

public class GetAllSubscriptionPaymentsQueryHandlerTest
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IPharmacyRepository _pharmacyRepo = A.Fake<IPharmacyRepository>();
    private readonly GetAllSubscriptionPaymentsQueryHandler _handler;

    public GetAllSubscriptionPaymentsQueryHandlerTest()
    {
        A.CallTo(() => _unitOfWork.Pharmacies).Returns(_pharmacyRepo);
        _handler = new GetAllSubscriptionPaymentsQueryHandler(_unitOfWork);
    }

    [Fact]
    public async Task Handle_ReturnsPendingPayments()
    {
        var payment = SubscriptionPayment.Create("pharm-1", "/images/payments/receipt.png");
        var list = new List<SubscriptionPayment> { payment };

        A.CallTo(() => _pharmacyRepo.GetPendingPaymentsAsync(A<CancellationToken>.Ignored))
            .Returns(list);

        var result = await _handler.Handle(new GetAllSubscriptionPaymentsQuery(), CancellationToken.None);

        Assert.NotNull(result);
        var item = Assert.Single(result);
        Assert.Equal("pharm-1", item.PharmacyId);
        Assert.Equal("/images/payments/receipt.png", item.ImageUrl);
        Assert.False(item.IsVerified);
    }
}
