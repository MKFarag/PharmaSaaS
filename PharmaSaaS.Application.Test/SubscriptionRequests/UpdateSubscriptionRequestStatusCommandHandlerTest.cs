using PharmaSaaS.Application.Features.SubscriptionRequests.UpdateStatus;

namespace PharmaSaaS.Application.Test.SubscriptionRequests;

public class UpdateSubscriptionRequestStatusCommandHandlerTest
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly ISubscriptionRequestRepository _subRepo = A.Fake<ISubscriptionRequestRepository>();
    private readonly IPharmacyRepository _pharmacyRepo = A.Fake<IPharmacyRepository>();
    private readonly INotificationService _notificationService = A.Fake<INotificationService>();
    private readonly UpdateSubscriptionRequestStatusCommandHandler _handler;

    public UpdateSubscriptionRequestStatusCommandHandlerTest()
    {
        A.CallTo(() => _unitOfWork.SubscriptionRequests).Returns(_subRepo);
        A.CallTo(() => _unitOfWork.Pharmacies).Returns(_pharmacyRepo);
        A.CallTo(() => _unitOfWork.SharedCompleteAsync(A<CancellationToken>.Ignored))
            .Returns(1);

        _handler = new UpdateSubscriptionRequestStatusCommandHandler(_unitOfWork, _notificationService);
    }

    [Fact]
    public async Task Handle_WhenSubscriptionRequestNotFound_ReturnsNotFoundError()
    {
        A.CallTo(() => _subRepo.GetAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns((SubscriptionRequest?)null);

        var command = new UpdateSubscriptionRequestStatusCommand("non-existent", new UpdateSubscriptionRequestStatusRequest(nameof(SubscriptionRequestStatus.Approved), null));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(SubscriptionRequestErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Handle_WhenApproved_ApprovesRequestAndCreatesPharmacy()
    {
        var subscriptionRequest = SubscriptionRequest.Create("New Pharmacy", "Owner", "123", "owner@test.com");
        subscriptionRequest.Id = "req-1";

        A.CallTo(() => _subRepo.GetAsync("req-1", A<CancellationToken>.Ignored))
            .Returns(subscriptionRequest);

        var command = new UpdateSubscriptionRequestStatusCommand("req-1", new UpdateSubscriptionRequestStatusRequest(nameof(SubscriptionRequestStatus.Approved), 6));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionRequestStatus.Approved, subscriptionRequest.Status);

        A.CallTo(() => _pharmacyRepo.AddAsync(
            A<Pharmacy>.That.Matches(p =>
                p.Name == "New Pharmacy" &&
                p.OwnerName == "Owner" &&
                p.PhoneNumber == "123" &&
                p.Email == "owner@test.com" &&
                p.SubscriptionStart == DateOnly.FromDateTime(DateTime.UtcNow) &&
                p.SubscriptionEnd == DateOnly.FromDateTime(DateTime.UtcNow).AddMonths(6) &&
                p.IsActive),
            A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _unitOfWork.SharedCompleteAsync(A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenRejected_RejectsRequest()
    {
        var subscriptionRequest = SubscriptionRequest.Create("Test", "Owner", "123", "test@test.com");
        subscriptionRequest.Id = "req-2";

        A.CallTo(() => _subRepo.GetAsync("req-2", A<CancellationToken>.Ignored))
            .Returns(subscriptionRequest);

        var command = new UpdateSubscriptionRequestStatusCommand("req-2", new UpdateSubscriptionRequestStatusRequest(nameof(SubscriptionRequestStatus.Rejected), null));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionRequestStatus.Rejected, subscriptionRequest.Status);

        A.CallTo(() => _unitOfWork.SharedCompleteAsync(A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenContacted_ContactsRequest()
    {
        var subscriptionRequest = SubscriptionRequest.Create("Test", "Owner", "123", "test@test.com");
        subscriptionRequest.Id = "req-3";

        A.CallTo(() => _subRepo.GetAsync("req-3", A<CancellationToken>.Ignored))
            .Returns(subscriptionRequest);

        var command = new UpdateSubscriptionRequestStatusCommand("req-3", new UpdateSubscriptionRequestStatusRequest(nameof(SubscriptionRequestStatus.Contacted), null));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionRequestStatus.Contacted, subscriptionRequest.Status);

        A.CallTo(() => _unitOfWork.SharedCompleteAsync(A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();
    }
}
