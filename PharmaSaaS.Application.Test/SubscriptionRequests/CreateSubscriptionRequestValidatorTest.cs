using PharmaSaaS.Application.Contracts.SubscriptionRequests;

namespace PharmaSaaS.Application.Test.SubscriptionRequests;

public class CreateSubscriptionRequestValidatorTest
{
    private readonly CreateSubscriptionRequestValidator _validator = new();

    [Theory]
    [InlineData("01012345678")]
    [InlineData("01112345678")]
    [InlineData("01212345678")]
    [InlineData("01512345678")]
    [InlineData("+201012345678")]
    [InlineData("+201234567890")]
    [InlineData("+966501234567")]
    public void Validate_WhenPhoneNumberIsValid_ReturnsSuccess(string phoneNumber)
    {
        var request = new CreateSubscriptionRequest("My Pharmacy", "John Doe", phoneNumber, "john@example.com");

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("invalid")]
    [InlineData("less than 20 chars")]
    [InlineData("010")]
    [InlineData("+0123456789")]
    public void Validate_WhenPhoneNumberIsInvalid_ReturnsValidationError(string phoneNumber)
    {
        var request = new CreateSubscriptionRequest("My Pharmacy", "John Doe", phoneNumber, "john@example.com");

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateSubscriptionRequest.PhoneNumber));
    }
}
