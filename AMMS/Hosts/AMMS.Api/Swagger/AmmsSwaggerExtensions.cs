using Microsoft.OpenApi.Models;

namespace AMMS.Api.Swagger;

internal static class AmmsSwaggerExtensions
{
    public static IServiceCollection AddAmmsSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "AMMS API",
                Version = "v1"
            });

            options.CustomSchemaIds(type => type.FullName?.Replace("+", ".", StringComparison.Ordinal));
            options.OperationFilter<AmmsAcceptLanguageHeaderOperationFilter>();
            options.OperationFilter<AmmsCreateRequestExampleOperationFilter>();
        });

        return services;
    }
}
