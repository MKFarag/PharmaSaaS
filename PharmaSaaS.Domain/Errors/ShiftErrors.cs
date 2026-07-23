namespace PharmaSaaS.Domain.Errors;

public record ShiftErrors
{
    public static readonly Error NotFound =
        new("Shift.NotFound", "No shift found with the specified ID", StatusCodes.NotFound);

    public static readonly Error AlreadyOpen =
        new("Shift.AlreadyOpen", "A shift is already open for today", StatusCodes.Conflict);

    public static readonly Error NotOpen =
        new("Shift.NotOpen", "The shift is not currently open", StatusCodes.Conflict);

    public static readonly Error AlreadyClosed =
        new("Shift.AlreadyClosed", "The shift is already closed", StatusCodes.Conflict);
}
