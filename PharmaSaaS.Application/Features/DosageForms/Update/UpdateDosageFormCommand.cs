namespace PharmaSaaS.Application.Features.DosageForms.Update;

public record UpdateDosageFormCommand(int Id, DosageFormRequest Request) : IRequest<Result>;
