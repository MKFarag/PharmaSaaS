using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Security.Claims;

namespace PharmaSaaS.Presentation;

internal static class GlobalExtensions
{
    extension(Result result)
    {
        internal ObjectResult ToProblem()
        {
            if (result.IsSuccess)
                throw new InvalidOperationException("Cannot create a problem details response for a successful result.");

            var problem = Results.Problem(statusCode: result.Error.StatusCode);

            var problemDetails = problem.GetType().GetProperty(nameof(ProblemDetails))!.GetValue(problem) as ProblemDetails;

            problemDetails!.Extensions = new Dictionary<string, object?>
            {
                {
                    "error", new Dictionary<string, string>
                    {
                        { "code", result.Error.Code },
                        { "description", result.Error.Description }
                    }
                }
            };

            return new ObjectResult(problemDetails);
        }
    }

    extension(ControllerBase controller)
    {
        internal IActionResult ToProblem(ValidationResult result)
        {
            var modelState = new ModelStateDictionary();

            foreach (var error in result.Errors)
                modelState.AddModelError(error.PropertyName, error.ErrorMessage);

            return controller.ValidationProblem(modelState);
        }
    }

    extension(ClaimsPrincipal user)
    {
        internal string? GetId() =>
            user.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    extension(IFormFile formFile)
    {
        internal FileData ToFileData()
            => new(formFile.FileName, formFile.ContentType, formFile.Length, formFile.OpenReadStream());
    }
}
