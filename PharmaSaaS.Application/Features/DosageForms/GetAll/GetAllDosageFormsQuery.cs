namespace PharmaSaaS.Application.Features.DosageForms.GetAll;

public record GetAllDosageFormsQuery : IRequest<IEnumerable<DosageFormResponse>>;
