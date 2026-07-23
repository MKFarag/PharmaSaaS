using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using System.Reflection;
using OpenApiParameter = NSwag.OpenApiParameter;

namespace PharmaSaaS.Presentation.SwaggerProcessors;

public class TenantHeaderOperationProcessor : IOperationProcessor
{
    public bool Process(OperationProcessorContext context)
    {
        var hasSystemOwner = context.MethodInfo?.GetCustomAttribute<SystemOwnerAttribute>() is not null
            || context.ControllerType?.GetCustomAttribute<SystemOwnerAttribute>() is not null;

        if (hasSystemOwner)
        {
            AddHeader(context, "X-System-Owner-Key", "System Owner API Key", isRequired: true);
        }
        else
        {
            AddHeader(context, "X-Tenant-Id", "Tenant ID", isRequired: false);
        }

        return true;
    }

    private static void AddHeader(OperationProcessorContext context, string name, string description, bool isRequired)
    {
        var existing = context.OperationDescription.Operation.Parameters
            .FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        if (existing is null)
        {
            context.OperationDescription.Operation.Parameters.Add(new OpenApiParameter
            {
                Name = name,
                Kind = OpenApiParameterKind.Header,
                Type = NJsonSchema.JsonObjectType.String,
                IsRequired = isRequired,
                Description = description,
            });
        }
    }
}
