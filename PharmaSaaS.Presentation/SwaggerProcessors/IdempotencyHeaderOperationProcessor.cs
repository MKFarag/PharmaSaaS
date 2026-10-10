using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using System.Reflection;
using OpenApiParameter = NSwag.OpenApiParameter;

namespace PharmaSaaS.Presentation.SwaggerProcessors;

public class IdempotencyHeaderOperationProcessor : IOperationProcessor
{
    public bool Process(OperationProcessorContext context)
    {
        var hasIdempotent = context.MethodInfo?.GetCustomAttribute<IdempotentAttribute>() is not null
            || context.ControllerType?.GetCustomAttribute<IdempotentAttribute>() is not null;

        if (hasIdempotent)
        {
            var existing = context.OperationDescription.Operation.Parameters
                .FirstOrDefault(p => p.Name.Equals(IdempotentAttribute._headerName, StringComparison.OrdinalIgnoreCase));

            if (existing is null)
            {
                context.OperationDescription.Operation.Parameters.Add(new OpenApiParameter
                {
                    Name = IdempotentAttribute._headerName,
                    Kind = OpenApiParameterKind.Header,
                    Type = NJsonSchema.JsonObjectType.String,
                    IsRequired = true,
                    Description = "Unique idempotency key to prevent duplicate request processing."
                });
            }
        }

        return true;
    }
}
