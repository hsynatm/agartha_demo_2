using AMMS.Infrastructure.Localization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using System.Text.Json.Nodes;

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

            options.AddOperationTransformer((operation, context, _) =>
            {
                AddAcceptLanguageHeader(operation);
                AddCreateRequestExample(operation, context);
                return Task.CompletedTask;
            });
        });

        return services;
    }

    private static void AddAcceptLanguageHeader(OpenApiOperation operation)
    {
        operation.Parameters ??= [];

        if (operation.Parameters.Any(parameter =>
                string.Equals(parameter.Name, "Accept-Language", StringComparison.OrdinalIgnoreCase)))
        {
            return;
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
    }

    private static void AddCreateRequestExample(OpenApiOperation operation, OpenApiOperationTransformerContext context)
    {
        if (!HttpMethods.IsPost(context.Description.HttpMethod ?? string.Empty))
        {
            return;
        }

        if (context.Description.ActionDescriptor is not ControllerActionDescriptor controllerAction)
        {
            return;
        }

        if (!string.Equals(controllerAction.ActionName, "Create", StringComparison.Ordinal))
        {
            return;
        }

        var exampleJson = AmmsOpenApiCreateExamples.Resolve(controllerAction.ControllerName);
        if (exampleJson is null)
        {
            return;
        }

        if (operation.RequestBody?.Content is null
            || !operation.RequestBody.Content.TryGetValue("application/json", out var mediaType))
        {
            mediaType = new OpenApiMediaType();
            operation.RequestBody = new OpenApiRequestBody
            {
                Required = true,
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = mediaType
                }
            };
        }

        mediaType.Example = JsonNode.Parse(exampleJson);
    }
}
