using PharmaSaaS.Application.Features.SubscriptionRequests.GetAll;

namespace PharmaSaaS.Application.Test.SubscriptionRequests;

public class GetAllSubscriptionRequestsQueryHandlerTest : IClassFixture<MapsterTestFixture>
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly ISubscriptionRequestRepository _subRepo = A.Fake<ISubscriptionRequestRepository>();
    private readonly GetAllSubscriptionRequestsQueryHandler _handler;

    public GetAllSubscriptionRequestsQueryHandlerTest(MapsterTestFixture _)
    {
        A.CallTo(() => _unitOfWork.SubscriptionRequests).Returns(_subRepo);

        _handler = new GetAllSubscriptionRequestsQueryHandler(_unitOfWork);
    }

    [Fact]
    public async Task Handle_ReturnsAllSubscriptionRequests_WhenStatusNotProvided()
    {
        var expected = new List<SubscriptionRequestResponse>
        {
            new("1", "Pharmacy A", "Owner A", "111", "a@test.com", DateTime.UtcNow, "Pending"),
            new("2", "Pharmacy B", "Owner B", "222", "b@test.com", DateTime.UtcNow, "Approved"),
        };

        A.CallTo(() => _subRepo.GetAllAsync<SubscriptionRequestResponse>(A<string?>.Ignored, A<CancellationToken>.Ignored))
            .Returns(expected);

        var result = await _handler.Handle(new GetAllSubscriptionRequestsQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count());
        Assert.Contains(result, r => r.Id == "1" && r.PharmacyName == "Pharmacy A");
        Assert.Contains(result, r => r.Id == "2" && r.PharmacyName == "Pharmacy B");
    }

    [Fact]
    public async Task Handle_FiltersByStatus_WhenStatusProvided()
    {
        var all = new List<SubscriptionRequestResponse>
        {
            new("1", "Pharmacy A", "Owner A", "111", "a@test.com", DateTime.UtcNow, "Pending"),
            new("2", "Pharmacy B", "Owner B", "222", "b@test.com", DateTime.UtcNow, "Approved"),
            new("3", "Pharmacy C", "Owner C", "333", "c@test.com", DateTime.UtcNow, "Pending"),
        };

        A.CallTo(() => _subRepo.GetAllAsync<SubscriptionRequestResponse>("Pending", A<CancellationToken>.Ignored))
            .Returns(all.Where(r => r.Status == "Pending").ToList());

        var result = await _handler.Handle(new GetAllSubscriptionRequestsQuery("Pending"), CancellationToken.None);

        Assert.Equal(2, result.Count());
        Assert.All(result, r => Assert.Equal("Pending", r.Status));
    }
}
