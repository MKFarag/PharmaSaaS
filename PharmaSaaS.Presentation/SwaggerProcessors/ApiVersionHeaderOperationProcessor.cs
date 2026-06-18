using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using OpenApiParameter = NSwag.OpenApiParameter;

namespace PharmaSaaS.Presentation.SwaggerProcessors;

public class ApiVersionHeaderOperationProcessor(ApiVersionDescription description) : IOperationProcessor
{
    private readonly string _version = description.ApiVersion.MajorVersion?.ToString() ?? "1";
    private const string _versionParameterName = "x-api-version";

    public bool Process(OperationProcessorContext context)
    {
        var existingParameter = context.OperationDescription.Operation.Parameters
            .FirstOrDefault(p => p.Name.Equals(_versionParameterName, StringComparison.OrdinalIgnoreCase));

        if (existingParameter != null)
        {
            existingParameter.Default = _version;
            existingParameter.Description = $"API Version";
        }
        else
        {
            context.OperationDescription.Operation.Parameters.Add(new OpenApiParameter
            {
                Name = "x-api-version",
                Kind = OpenApiParameterKind.Header,
                Type = NJsonSchema.JsonObjectType.String,
                IsRequired = true,
                Default = _version,
                Description = $"API Version",
                Schema = new NJsonSchema.JsonSchema
                {
                    Type = NJsonSchema.JsonObjectType.String,
                    Default = _version
                }
            });
        }

        return true;
    }
}