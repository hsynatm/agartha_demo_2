using AMMS.Infrastructure.Json;
using System.Text.Json.Serialization;

namespace AMMS.Api.Json;

internal static class AmmsJsonExtensions
{
    public static IMvcBuilder AddAmmsJsonOptions(this IMvcBuilder builder)
    {
        builder.AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new EmptyStringToGuidJsonConverter());
            options.JsonSerializerOptions.Converters.Add(new EmptyStringToNullableGuidJsonConverter());
            options.JsonSerializerOptions.Converters.Add(new EmptyStringToDateTimeJsonConverter());
            options.JsonSerializerOptions.Converters.Add(new EmptyStringToNullableDateTimeJsonConverter());
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        return builder;
    }
}
