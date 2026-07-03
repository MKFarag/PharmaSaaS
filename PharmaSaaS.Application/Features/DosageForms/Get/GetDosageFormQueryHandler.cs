namespace PharmaSaaS.Application.Features.DosageForms.Get;

public class GetDosageFormQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetDosageFormQuery, Result<DosageFormResponse>>
{
    public async Task<Result<DosageFormResponse>> Handle(GetDosageFormQuery request, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.DosageForms.FindAsync(d => d.Id == request.Id, cancellationToken) is not { } dosageForm)
            return Result.Failure<DosageFormResponse>(DosageFormErrors.NotFound);

        return Result.Success(dosageForm.Adapt<DosageFormResponse>());
    }
}