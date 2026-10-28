namespace PharmaSaaS.Presentation.Contracts.Payments;

public record UploadPaymentRequest(IFormFile Image);

#region Validation

public class UploadPaymentRequestValidator : AbstractValidator<UploadPaymentRequest>
{
    public UploadPaymentRequestValidator()
    {
        RuleFor(x => x.Image)
            .NotNull()
            .WithMessage("Payment proof image is required.");

        RuleFor(x => x.Image)
            .SetValidator(new ImageFileSizeValidator())
            .SetValidator(new ImageBlockedSignaturesValidator())
            .SetValidator(new ImageExtensionValidator())
            .When(x => x.Image is not null);
    }
}

#endregion
