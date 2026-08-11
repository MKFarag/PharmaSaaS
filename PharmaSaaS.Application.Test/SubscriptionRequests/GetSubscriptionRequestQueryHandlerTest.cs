using PharmaSaaS.Application.Features.SubscriptionRequests.Get;

using PharmaSaaS.Domain.Enums;

namespace PharmaSaaS.Application.Test.SubscriptionRequests;

public class GetSubscriptionRequestQueryHandlerTest : IClassFixture<MapsterTestFixture>
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly ISubscriptionRequestRepository _subRepo = A.Fake<ISubscriptionRequestRepository>();
    private readonly GetSubscriptionRequestQueryHandler _handler;

    public GetSubscriptionRequestQueryHandlerTest(MapsterTestFixture _)
    {
        A.CallTo(() => _unitOfWork.SubscriptionRequests).Returns(_subRepo);

        _handler = new GetSubscriptionRequestQueryHandler(_unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenSubscriptionRequestFound_ReturnsSuccess()
    {
        var subscriptionRequest = SubscriptionRequest.Create("Test Pharmacy", "John Doe", "+201234567890", "john@example.com");

        A.CallTo(() => _subRepo.GetAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns(subscriptionRequest);

        var result = await _handler.Handle(new GetSubscriptionRequestQuery("some-id"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Test Pharmacy", result.Value.PharmacyName);
        Assert.Equal("John Doe", result.Value.OwnerName);
        Assert.Equal("john@example.com", result.Value.Email);
        Assert.Equal(SubscriptionRequestStatus.Pending, result.Value.Status);
    }

    [Fact]
    public async Task Handle_WhenSubscriptionRequestNotFound_ReturnsNotFoundError()
    {
        A.CallTo(() => _subRepo.GetAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns((SubscriptionRequest?)null);

        var result = await _handler.Handle(new GetSubscriptionRequestQuery("non-existent"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(SubscriptionRequestErrors.NotFound, result.Error);
    }
}
