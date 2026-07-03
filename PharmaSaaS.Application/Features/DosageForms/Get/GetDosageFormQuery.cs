namespace PharmaSaaS.Application.Features.DosageForms.Get;

public record GetDosageFormQuery(int Id) : IRequest<Result<DosageFormResponse>>;
