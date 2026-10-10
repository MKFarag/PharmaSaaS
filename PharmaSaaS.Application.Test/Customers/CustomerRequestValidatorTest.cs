using PharmaSaaS.Application.Contracts.Customers;

namespace PharmaSaaS.Application.Test.Customers;

public class CustomerRequestValidatorTest
{
    private readonly CustomerRequestValidator _validator = new();

    [Theory]
    [InlineData("01012345678")]
    [InlineData("01112345678")]
    [InlineData("01212345678")]
    [InlineData("01512345678")]
    [InlineData("+201012345678")]
    [InlineData("+201234567890")]
    [InlineData("0223456789")]
    [InlineData("0501234567")]
    [InlineData("+966501234567")]
    [InlineData("00966501234567")]
    public void Validate_WhenPhoneNumberIsValid_ReturnsSuccess(string phoneNumber)
    {
        var request = new CustomerRequest("John Doe", phoneNumber);

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abc")]
    [InlineData("random text")]
    [InlineData("010")]
    [InlineData("013123456789")]
    [InlineData("+0123456789")]
    [InlineData("010-1234-5678")]
    [InlineData("010 1234 5678")]
    public void Validate_WhenPhoneNumberIsInvalid_ReturnsValidationError(string phoneNumber)
    {
        var request = new CustomerRequest("John Doe", phoneNumber);

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CustomerRequest.PhoneNumber));
    }
}
