using AMMS.Infrastructure.Localization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace AMMS.Api.OpenApi;

internal static class AmmsOpenApiExtensions
{
    public static IServiceCollection AddAmmsOpenApi(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.CreateSchemaReferenceId = jsonTypeInfo =>
                jsonTypeInfo.Type.FullName?.Replace("+", ".", StringComparison.Ordinal);

            options.AddDocumentTransformer((document, _, _) =>
            {
                document.Info = new OpenApiInfo
                {
                    Title = "AMMS API",
                    Version = "v1"
                };
                return Task.CompletedTask;
            });

            options.AddOperationTransformer((operation, _, _) =>
            {
                operation.Parameters ??= [];

                if (operation.Parameters.Any(parameter =>
                        string.Equals(parameter.Name, "Accept-Language", StringComparison.OrdinalIgnoreCase)))
                {
                    return Task.CompletedTask;
                }

                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "Accept-Language",
                    In = ParameterLocation.Header,
                    Description = "Response language for ProblemDetails (errors/validation). Supported: tr, en, ar. Default: tr.",
                    Required = false,
                    Schema = new OpenApiSchema
                    {
                        Type = JsonSchemaType.String,
                        Default = CultureConstants.DefaultCulture,
                        Example = CultureConstants.DefaultCulture
                    }
                });

                return Task.CompletedTask;
            });
        });

        return services;
    }
}
