using AMMS.Core.Exceptions;
using AMMS.Infrastructure.Authentication;
using AMMS.Infrastructure.Localization;
using AMMS.Infrastructure.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Runtime.ExceptionServices;
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

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment environment, JsonStringLocalizer localizer)
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
                ExceptionDispatchInfo.Capture(exception).Throw();
                return;
            }

            var culture = GetCulture(context);
            var mapped = MapException(exception, _environment, _localizer, culture);
            LogRequestFailed(context, mapped, exception);

            await WriteProblemDetailsAsync(context, mapped.StatusCode, CreateProblemDetails(context, mapped, culture));
        }

        private async Task WriteHttpErrorIfNeededAsync(HttpContext context)
        {
            if (context.Response.StatusCode is < StatusCodes.Status400BadRequest)
            {
                return;
            }

            if (SessionTerminatedContext.IsTerminated(context))
            {
                context.RequestServices
                    .GetRequiredService<RequestFailureLogger>()
                    .LogSessionTerminated(
                        context,
                        SessionTerminatedContext.ErrorCode,
                        SessionTerminatedContext.Message);
                return;
            }

            if (context.Response.HasStarted || context.Response.ContentLength > 0)
            {
                LogExistingHttpError(context);
                return;
            }

            var culture = GetCulture(context);
            var mapped = MapHttpStatus(context.Response.StatusCode, _localizer, culture);
            LogRequestFailed(context, mapped, exception: null);
            await WriteProblemDetailsAsync(context, mapped.StatusCode, CreateProblemDetails(context, mapped, culture));
        }

        private void LogExistingHttpError(HttpContext context)
        {
            if (context.Response.StatusCode is < StatusCodes.Status400BadRequest)
            {
                return;
            }

            var culture = GetCulture(context);
            var mapped = MapHttpStatus(context.Response.StatusCode, _localizer, culture);
            var failure = AuthorizationFailureContext.Get(context);

            if (IsAuthorizationFailure(mapped.StatusCode, exception: null) && failure is not null)
            {
                context.RequestServices
                    .GetRequiredService<AuthorizationFailureLogger>()
                    .Log(context, mapped.StatusCode, mapped.ErrorCode, failure.Reason);
                return;
            }

            LogRequestFailed(context, mapped, exception: null);
        }

        private void LogRequestFailed(HttpContext context, MappedException mapped, Exception? exception)
        {
            context.Response.StatusCode = mapped.StatusCode;

            if (IsAuthorizationFailure(mapped.StatusCode, exception))
            {
                context.RequestServices
                    .GetRequiredService<AuthorizationFailureLogger>()
                    .Log(context, mapped.StatusCode, mapped.ErrorCode, mapped.Detail, exception);
                return;
            }

            context.RequestServices
                .GetRequiredService<RequestFailureLogger>()
                .Log(
                    context,
                    mapped.StatusCode,
                    mapped.ErrorCode,
                    mapped.LocalizationKey,
                    mapped.Title,
                    mapped.Detail,
                    exception);
        }

        private static bool IsAuthorizationFailure(int statusCode, Exception? exception) =>
            statusCode is StatusCodes.Status401Unauthorized or StatusCodes.Status403Forbidden
            || exception is AmmsException.Unauthorized or AmmsException.Forbidden or UnauthorizedAccessException;

        private static string GetCulture(HttpContext context) =>
            context.Items[CultureConstants.HttpContextItemKey] as string ?? CultureConstants.DefaultCulture;

        private static ProblemDetails CreateProblemDetails(HttpContext context, MappedException mapped, string culture)
        {
            var correlationId = CorrelationIdResolver.Resolve(context) ?? context.TraceIdentifier;

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
            if (context.Response.HasStarted)
            {
                return;
            }

            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json";

            await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, JsonOptions));
        }
    }






}
