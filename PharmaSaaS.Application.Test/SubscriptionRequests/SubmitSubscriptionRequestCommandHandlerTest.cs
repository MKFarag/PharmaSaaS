using PharmaSaaS.Application.Features.SubscriptionRequests.Create;

using PharmaSaaS.Domain.Enums;

namespace PharmaSaaS.Application.Test.SubscriptionRequests;

public class SubmitSubscriptionRequestCommandHandlerTest
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly ISubscriptionRequestRepository _subRepo = A.Fake<ISubscriptionRequestRepository>();
    private readonly INotificationService _notificationService = A.Fake<INotificationService>();
    private readonly SubmitSubscriptionRequestCommandHandler _handler;

    public SubmitSubscriptionRequestCommandHandlerTest()
    {
        A.CallTo(() => _unitOfWork.SubscriptionRequests).Returns(_subRepo);
        A.CallTo(() => _subRepo.ExistsPendingAsync(A<string>.Ignored, A<string>.Ignored, A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns(false);
        A.CallTo(() => _unitOfWork.SharedCompleteAsync(A<CancellationToken>.Ignored))
            .Returns(1);

        _handler = new SubmitSubscriptionRequestCommandHandler(_unitOfWork, _notificationService);
    }

    [Fact]
    public async Task Handle_ValidRequest_CreatesAndSavesSubscriptionRequest()
    {
        var request = new CreateSubscriptionRequest("Test Pharmacy", "John Doe", "+201234567890", "john@example.com");
        var command = new SubmitSubscriptionRequestCommand(request);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);

        A.CallTo(() => _subRepo.AddAsync(A<SubscriptionRequest>.That.Matches(
            s => s.PharmacyName == "Test Pharmacy" &&
                 s.OwnerName == "John Doe" &&
                 s.PhoneNumber == "+201234567890" &&
                 s.Email == "john@example.com" &&
                 s.Status == SubscriptionRequestStatus.Pending), A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _unitOfWork.SharedCompleteAsync(A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenPendingRequestAlreadyExists_ReturnsAlreadyPendingError()
    {
        A.CallTo(() => _subRepo.ExistsPendingAsync("Existing Pharmacy", "111", "e@e.com", A<CancellationToken>.Ignored))
            .Returns(true);

        var request = new CreateSubscriptionRequest("Existing Pharmacy", "Owner", "111", "e@e.com");
        var command = new SubmitSubscriptionRequestCommand(request);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(SubscriptionRequestErrors.AlreadyPending, result.Error);
    }
}
