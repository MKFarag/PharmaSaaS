using System.Security.Cryptography;
using System.Text;

namespace PharmaSaaS.Presentation.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class SystemOwnerAttribute : Attribute, IAuthorizationFilter
{
    private const string _headerName = "X-System-Owner-Key";

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var configuration = context.HttpContext
            .RequestServices
            .GetRequiredService<IConfiguration>();

        var configuredApiKey = configuration["SystemOwnerApiKey"];

        if (string.IsNullOrEmpty(configuredApiKey))
        {
            context.Result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
            return;
        }

        if (!context.HttpContext.Request.Headers.TryGetValue(_headerName, out var providedApiKey))
        {
            context.Result = new UnauthorizedObjectResult("System Owner API Key is missing.");
            return;
        }

        var configuredKeyBytes = Encoding.UTF8.GetBytes(configuredApiKey);
        var providedKeyBytes = Encoding.UTF8.GetBytes(providedApiKey.ToString());

        if (!CryptographicOperations.FixedTimeEquals(configuredKeyBytes, providedKeyBytes))
        {
            context.Result = new UnauthorizedObjectResult("Invalid System Owner API Key.");
            return;
        }
    }
}