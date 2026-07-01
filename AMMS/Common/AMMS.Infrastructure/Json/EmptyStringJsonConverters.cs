using System.Text.Json;
using System.Text.Json.Serialization;

namespace AMMS.Infrastructure.Json;


public sealed class EmptyStringToGuidJsonConverter : JsonConverter<Guid>
{
    public override Guid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.Null => Guid.Empty,
            JsonTokenType.String => ParseGuid(reader.GetString()),
            _ => reader.GetGuid()
        };
    }

    public override void Write(Utf8JsonWriter writer, Guid value, JsonSerializerOptions options) => writer.WriteStringValue(value);

    private static Guid ParseGuid(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Guid.Empty;
        }

        return Guid.TryParse(value, out var guid) ? guid : Guid.Empty;
    }
}

public sealed class EmptyStringToNullableGuidJsonConverter : JsonConverter<Guid?>
{
    public override Guid? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.Null => null,
            JsonTokenType.String => ParseNullableGuid(reader.GetString()),
            _ => reader.GetGuid()
        };
    }

    public override void Write(Utf8JsonWriter writer, Guid? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStringValue(value.Value);
    }

    private static Guid? ParseNullableGuid(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : Guid.TryParse(value, out var guid) ? guid : null;
}

public sealed class EmptyStringToDateTimeJsonConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.Null => default,
            JsonTokenType.String => ParseDateTime(reader.GetString()),
            _ => reader.GetDateTime()
        };
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options) => writer.WriteStringValue(value);

    private static DateTime ParseDateTime(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return default;
        }

        return DateTime.TryParse(value, out var dateTime) ? dateTime : default;
    }
}

public sealed class EmptyStringToNullableDateTimeJsonConverter : JsonConverter<DateTime?>
{
    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.Null => null,
            JsonTokenType.String => ParseNullableDateTime(reader.GetString()),
            _ => reader.GetDateTime()
        };
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStringValue(value.Value);
    }

    private static DateTime? ParseNullableDateTime(string? value) => string.IsNullOrWhiteSpace(value) ? null : DateTime.TryParse(value, out var dateTime) ? dateTime : null;
}
