using Microsoft.AspNetCore.Authentication.JwtBearer;
using NSwag;
using NSwag.Generation;
using NSwag.Generation.AspNetCore;
using NSwag.Generation.Processors.Security;

namespace PharmaSaaS.Presentation.SwaggerProcessors;

public static class SwaggerExtensions
{
    public static OpenApiDocumentGeneratorSettings AddBearerSecurity(this AspNetCoreOpenApiDocumentGeneratorSettings options)
    {
        options.AddSecurity(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
        {
            Type = OpenApiSecuritySchemeType.Http,
            Scheme = JwtBearerDefaults.AuthenticationScheme.ToLower(),
            BearerFormat = "JWT",
            Description = "Please add your token"
        });

        options.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor(JwtBearerDefaults.AuthenticationScheme));

        return options;
    }

    public static OpenApiDocumentGeneratorSettings ConfigureDocument(this AspNetCoreOpenApiDocumentGeneratorSettings options, ApiVersionDescription description)
    {
        options.DocumentName = description.GroupName;
        options.Title = "PharmaSaaS";
        options.Version = description.ApiVersion.ToString();
        options.Description = $"API Description.{(description.IsDeprecated ? " This API version has been deprecated." : string.Empty)}";
        options.ApiGroupNames = [description.GroupName];

        return options;
    }
}
