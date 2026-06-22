using AMMS.Infrastructure.Localization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json;

namespace AMMS.Api.Swagger;

internal sealed class AmmsAcceptLanguageHeaderOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
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
                Type = "string",
                Default = new OpenApiString(CultureConstants.DefaultCulture),
                Example = new OpenApiString(CultureConstants.DefaultCulture)
            }
        });
    }
}

internal sealed class AmmsCreateRequestExampleOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (!HttpMethods.IsPost(context.ApiDescription.HttpMethod ?? string.Empty))
        {
            return;
        }

        if (context.ApiDescription.ActionDescriptor is not ControllerActionDescriptor controllerAction)
        {
            return;
        }

        if (!string.Equals(controllerAction.ActionName, "Create", StringComparison.Ordinal))
        {
            return;
        }

        var exampleJson = AmmsSwaggerCreateExamples.Resolve(controllerAction.ControllerName);
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

        mediaType.Example = ParseOpenApiAny(JsonDocument.Parse(exampleJson).RootElement);
    }

    private static IOpenApiAny? ParseOpenApiAny(JsonElement element) =>
        element.ValueKind switch
        {
            JsonValueKind.Object => ParseOpenApiObject(element),
            JsonValueKind.Array => ParseOpenApiArray(element),
            JsonValueKind.String => new OpenApiString(element.GetString()),
            JsonValueKind.Number when element.TryGetInt64(out var longValue) => new OpenApiLong(longValue),
            JsonValueKind.Number => new OpenApiDouble(element.GetDouble()),
            JsonValueKind.True => new OpenApiBoolean(true),
            JsonValueKind.False => new OpenApiBoolean(false),
            JsonValueKind.Null => null,
            _ => null
        };

    private static OpenApiObject ParseOpenApiObject(JsonElement element)
    {
        var obj = new OpenApiObject();
        foreach (var property in element.EnumerateObject())
        {
            var value = ParseOpenApiAny(property.Value);
            if (value is not null)
            {
                obj[property.Name] = value;
            }
        }

        return obj;
    }

    private static OpenApiArray ParseOpenApiArray(JsonElement element)
    {
        var array = new OpenApiArray();
        foreach (var item in element.EnumerateArray())
        {
            var value = ParseOpenApiAny(item);
            if (value is not null)
            {
                array.Add(value);
            }
        }

        return array;
    }
}
