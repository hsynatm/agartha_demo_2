using AMMS.Shared.Constants;
using Microsoft.AspNetCore.Http;

namespace AMMS.Infrastructure.Logging;

public sealed record AmmsLogSchemaSnapshot
{
    public string ErrorCode { get; init; } = GraylogSchemaDefaults.String;

    public string LocalizationKey { get; init; } = GraylogSchemaDefaults.String;

    public string Title { get; init; } = GraylogSchemaDefaults.String;

    public string Detail { get; init; } = GraylogSchemaDefaults.String;

    public int StatusCode { get; init; } = GraylogSchemaDefaults.StatusCode;

    public double Elapsed { get; init; } = GraylogSchemaDefaults.Elapsed;

    public long ElapsedMilliseconds { get; init; } = GraylogSchemaDefaults.ElapsedMilliseconds;
}

public static class AmmsLogSchemaContext
{
    public const string HttpContextItemKey = "Amms.LogSchema";

    public static void SetFromMapped(
        HttpContext context,
        string errorCode,
        string localizationKey,
        string title,
        string detail,
        int statusCode)
    {
        var current = Get(context);
        context.Items[HttpContextItemKey] = current with
        {
            ErrorCode = NullIfEmpty(errorCode) ?? GraylogSchemaDefaults.String,
            LocalizationKey = NullIfEmpty(localizationKey) ?? GraylogSchemaDefaults.String,
            Title = NullIfEmpty(title) ?? GraylogSchemaDefaults.String,
            Detail = NullIfEmpty(detail) ?? GraylogSchemaDefaults.String,
            StatusCode = statusCode
        };
    }

    public static void SetFromRequest(
        HttpContext context,
        int statusCode,
        double elapsedMilliseconds)
    {
        var elapsed = elapsedMilliseconds;
        var current = Get(context);
        var snapshot = current with
        {
            StatusCode = statusCode,
            Elapsed = elapsed,
            ElapsedMilliseconds = (long)elapsed
        };

        if (statusCode < StatusCodes.Status400BadRequest)
        {
            snapshot = snapshot with
            {
                ErrorCode = GraylogSchemaDefaults.String,
                LocalizationKey = GraylogSchemaDefaults.String,
                Title = GraylogSchemaDefaults.String,
                Detail = GraylogSchemaDefaults.String
            };
        }

        context.Items[HttpContextItemKey] = snapshot;
    }

    public static AmmsLogSchemaSnapshot Get(HttpContext? context)
    {
        if (context?.Items[HttpContextItemKey] is AmmsLogSchemaSnapshot snapshot)
        {
            return snapshot;
        }

        return new AmmsLogSchemaSnapshot();
    }

    private static string? NullIfEmpty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;
}
