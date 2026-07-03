namespace PharmaSaaS.Application.Features.DosageForms.Add;

public record AddDosageFormCommand(DosageFormRequest Request) : IRequest<Result<DosageFormResponse>>;
