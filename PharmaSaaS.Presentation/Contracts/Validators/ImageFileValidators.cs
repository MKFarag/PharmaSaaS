namespace PharmaSaaS.Presentation.Contracts.Validators;

public class ImageFileSizeValidator : AbstractValidator<IFormFile>
{
    public ImageFileSizeValidator()
    {
        RuleFor(x => x)
            .Must(f => f.Length <= FileSettings.MaxFileSizeInBytes)
            .WithMessage($"Image size must not exceed {FileSettings.MaxFileSizeInMb} MB.")
            .When(x => x is not null);
    }
}

public class ImageBlockedSignaturesValidator : AbstractValidator<IFormFile>
{
    public ImageBlockedSignaturesValidator()
    {
        RuleFor(x => x)
            .Must(f =>
            {
                using var reader = new BinaryReader(f.OpenReadStream());
                var bytes = reader.ReadBytes(2);
                var hex = BitConverter.ToString(bytes);
                return !FileSettings.BlockedSignatures.Any(sig => sig.Equals(hex, StringComparison.OrdinalIgnoreCase));
            })
            .WithMessage("File content is not allowed.")
            .When(x => x is not null);
    }
}

public class ImageExtensionValidator : AbstractValidator<IFormFile>
{
    public ImageExtensionValidator()
    {
        RuleFor(x => x)
            .Must(f => FileSettings.AllowedImagesExtensions.Contains(Path.GetExtension(f.FileName).ToLower()))
            .WithMessage($"Only {string.Join(", ", FileSettings.AllowedImagesExtensions)} images are allowed.")
            .When(x => x is not null);
    }
}

public class PrescriptionImageValidator : AbstractValidator<IFormFile>
{
    public PrescriptionImageValidator()
    {
        RuleFor(x => x)
            .NotNull()
            .WithMessage("Prescription image is required.");

        RuleFor(x => x)
            .SetValidator(new ImageFileSizeValidator())
            .SetValidator(new ImageBlockedSignaturesValidator())
            .SetValidator(new ImageExtensionValidator())
            .When(x => x is not null);
    }
}
