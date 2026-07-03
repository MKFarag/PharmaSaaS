namespace PharmaSaaS.Application.Features.DosageForms.Delete;

public record DeleteDosageFormCommand(int Id) : IRequest<Result>;
