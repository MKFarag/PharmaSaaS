namespace PharmaSaaS.Domain.Errors;

public record AIErrors
{
    public static readonly Error MonthlyLimitReached =
        new("AI.MonthlyLimitReached", "Monthly AI usage limit reached. Provide your own OpenRouter API key to continue.", StatusCodes.TooManyRequests);

    public static readonly Error ServiceUnavailable =
        new("AI.ServiceUnavailable", "The AI service is currently unavailable. Please try again later.", StatusCodes.ServiceUnavailable);

    public static readonly Error ParseFailed =
        new("AI.ParseFailed", "The AI returned an unexpected response format. Please try again.", StatusCodes.InternalServerError);
}
