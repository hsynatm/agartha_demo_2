using AMMS.Infrastructure.Localization;
using AMMS.Infrastructure.Logging;
using AMMS.Shared.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AMMS.Infrastructure.Middleware
{

    public sealed partial class ExceptionHandlingMiddleware
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IHostEnvironment _environment;
        private readonly JsonStringLocalizer _localizer;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger,
            IHostEnvironment environment,
            JsonStringLocalizer localizer)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
            _localizer = localizer;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
                await WriteHttpErrorIfNeededAsync(context);
            }
            catch (Exception exception)
            {
                await HandleExceptionAsync(context, exception);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            if (context.Response.HasStarted)
            {
                throw exception;
            }

            var culture = GetCulture(context);
            var mapped = MapException(exception, _environment, _localizer, culture);

            var moduleName = ApiModuleNameResolver.ResolveFromPath(context.Request.Path.Value) ?? "Unknown";

            if (mapped.StatusCode >= StatusCodes.Status500InternalServerError)
            {
                _logger.LogError(
                    exception,
                    "Request failed. {ModuleName} {ErrorCode} {LocalizationKey} {StatusCode} {TraceId} {RequestPath} {RequestMethod}",
                    moduleName,
                    mapped.ErrorCode,
                    mapped.LocalizationKey,
                    mapped.StatusCode,
                    context.TraceIdentifier,
                    context.Request.Path.Value,
                    context.Request.Method);
            }
            else
            {
                _logger.LogWarning(
                    exception,
                    "Request failed. {ModuleName} {ErrorCode} {LocalizationKey} {StatusCode} {TraceId} {RequestPath} {RequestMethod}",
                    moduleName,
                    mapped.ErrorCode,
                    mapped.LocalizationKey,
                    mapped.StatusCode,
                    context.TraceIdentifier,
                    context.Request.Path.Value,
                    context.Request.Method);
            }

            await WriteProblemDetailsAsync(context, mapped.StatusCode, CreateProblemDetails(context, mapped, culture));
        }

        private async Task WriteHttpErrorIfNeededAsync(HttpContext context)
        {
            if (context.Response.HasStarted || context.Response.ContentLength > 0)
            {
                return;
            }

            if (context.Response.StatusCode is < StatusCodes.Status400BadRequest)
            {
                return;
            }

            var culture = GetCulture(context);
            var mapped = MapHttpStatus(context.Response.StatusCode, _localizer, culture);
            await WriteProblemDetailsAsync(context, mapped.StatusCode, CreateProblemDetails(context, mapped, culture));
        }

        private static string GetCulture(HttpContext context) =>
            context.Items[CultureConstants.HttpContextItemKey] as string ?? CultureConstants.DefaultCulture;

        private static ProblemDetails CreateProblemDetails(HttpContext context, MappedException mapped, string culture)
        {
            var correlationId = context.Items[CorrelationIdConstants.HttpContextItemKey] as string
                ?? context.Request.Headers[CorrelationIdConstants.HeaderName].FirstOrDefault()
                ?? context.TraceIdentifier;

            var problem = new ProblemDetails
            {
                Status = mapped.StatusCode,
                Title = mapped.Title,
                Detail = mapped.Detail,
                Instance = context.Request.Path.Value
            };

            problem.Extensions["errorCode"] = mapped.ErrorCode;
            problem.Extensions["localizationKey"] = mapped.LocalizationKey;
            problem.Extensions["culture"] = culture;
            problem.Extensions["traceId"] = context.TraceIdentifier;
            problem.Extensions["correlationId"] = correlationId;

            if (mapped.MessageArgs is not null)
            {
                problem.Extensions["messageArgs"] = mapped.MessageArgs;
            }

            if (mapped.Errors is not null)
            {
                problem.Extensions["errors"] = mapped.Errors;

                if (mapped.Errors is System.Collections.IDictionary errorsByField)
                {
                    problem.Extensions["invalidFields"] = errorsByField.Keys
                        .Cast<object>()
                        .Select(key => key.ToString())
                        .Where(key => key is not null)
                        .ToArray();
                }
            }

            return problem;
        }

        private static async Task WriteProblemDetailsAsync(HttpContext context, int statusCode, ProblemDetails problemDetails)
        {
            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, JsonOptions));
        }
    }






}
