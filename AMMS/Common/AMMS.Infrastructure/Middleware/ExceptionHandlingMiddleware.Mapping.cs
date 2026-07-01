using AMMS.Core.Exceptions;
using AMMS.Core.Localization;
using AMMS.Infrastructure.Localization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace AMMS.Infrastructure.Middleware
{

    public sealed partial class ExceptionHandlingMiddleware
    {
        private static MappedException MapException(Exception exception,IHostEnvironment environment,JsonStringLocalizer localizer,string culture)
        {
            return exception switch
            {
                AmmsException.Validation validationException => MapValidation(validationException, localizer, culture),

                AmmsException.Business businessException => MapAmmsException(
                    StatusCodes.Status400BadRequest,
                    businessException,
                    LocalizationKeys.Shared.TitleBusiness,
                    localizer,
                    culture),

                AmmsException.NotFound notFoundException => MapAmmsException(
                    StatusCodes.Status404NotFound,
                    notFoundException,
                    LocalizationKeys.Shared.TitleNotFound,
                    localizer,
                    culture),

                AmmsException.Unauthorized unauthorizedException => MapAmmsException(
                    StatusCodes.Status401Unauthorized,
                    unauthorizedException,
                    LocalizationKeys.Shared.TitleUnauthorized,
                    localizer,
                    culture),

                AmmsException.Forbidden forbiddenException => MapAmmsException(
                    StatusCodes.Status403Forbidden,
                    forbiddenException,
                    LocalizationKeys.Shared.TitleForbidden,
                    localizer,
                    culture),

                UnauthorizedAccessException => MapSystemException(
                    StatusCodes.Status401Unauthorized,
                    "UNAUTHORIZED",
                    LocalizationKeys.Shared.Unauthorized,
                    LocalizationKeys.Shared.TitleUnauthorized,
                    localizer,
                    culture),

                ArgumentException => MapSystemException(
                    StatusCodes.Status400BadRequest,
                    "VALIDATION_ERROR",
                    LocalizationKeys.Shared.Invalid,
                    LocalizationKeys.Shared.TitleValidation,
                    localizer,
                    culture),

                KeyNotFoundException => MapSystemException(
                    StatusCodes.Status404NotFound,
                    "NOT_FOUND",
                    LocalizationKeys.Shared.NotFound,
                    LocalizationKeys.Shared.TitleNotFound,
                    localizer,
                    culture),

                _ => MapUnhandled(exception, environment, localizer, culture)
            };
        }

        private static MappedException MapHttpStatus(int statusCode, JsonStringLocalizer localizer, string culture)
        {
            var (errorCode, titleKey, detailKey) = statusCode switch
            {
                StatusCodes.Status400BadRequest => ("BAD_REQUEST", LocalizationKeys.Shared.TitleBadRequest, LocalizationKeys.Shared.BadRequest),
                StatusCodes.Status401Unauthorized => ("UNAUTHORIZED", LocalizationKeys.Shared.TitleUnauthorized, LocalizationKeys.Shared.Unauthorized),
                StatusCodes.Status403Forbidden => ("FORBIDDEN", LocalizationKeys.Shared.TitleForbidden, LocalizationKeys.Shared.Forbidden),
                StatusCodes.Status404NotFound => ("NOT_FOUND", LocalizationKeys.Shared.TitleNotFound, LocalizationKeys.Shared.NotFound),
                _ => ("HTTP_ERROR", LocalizationKeys.Shared.TitleHttpError, LocalizationKeys.Shared.HttpError)
            };

            return new MappedException(
                statusCode,
                errorCode,
                localizer.GetString(titleKey, culture),
                localizer.GetString(detailKey, culture),
                detailKey,
                null,
                null);
        }

        private static MappedException MapValidation(AmmsException.Validation validationException,JsonStringLocalizer localizer,string culture)
        {
            return new MappedException(
                StatusCodes.Status400BadRequest,
                validationException.ErrorCode,
                localizer.GetString(LocalizationKeys.Shared.TitleValidation, culture),
                localizer.GetString(validationException.LocalizationKey, culture, validationException.MessageArgs),
                validationException.LocalizationKey,
                validationException.MessageArgs,
                LocalizeValidationErrors(validationException.Details, localizer, culture));
        }

        private static MappedException MapAmmsException(int statusCode,AmmsException exception,string titleKey,JsonStringLocalizer localizer,string culture)
        {
            return new MappedException(
                statusCode,
                exception.ErrorCode,
                localizer.GetString(titleKey, culture),
                localizer.GetString(exception.LocalizationKey, culture, exception.MessageArgs),
                exception.LocalizationKey,
                exception.MessageArgs,
                exception.Details);
        }

        private static MappedException MapSystemException(int statusCode,string errorCode,string detailKey,string titleKey,JsonStringLocalizer localizer,string culture)
        {
            return new MappedException(
                statusCode,
                errorCode,
                localizer.GetString(titleKey, culture),
                localizer.GetString(detailKey, culture),
                detailKey,
                null,
                null);
        }

        private static MappedException MapUnhandled(Exception exception,IHostEnvironment environment,JsonStringLocalizer localizer,string culture)
        {
            var detail = environment.IsDevelopment()
                ? exception.Message
                : localizer.GetString(LocalizationKeys.Shared.Internal, culture);

            return new MappedException(
                StatusCodes.Status500InternalServerError,
                "INTERNAL_ERROR",
                localizer.GetString(LocalizationKeys.Shared.TitleInternal, culture),
                detail,
                LocalizationKeys.Shared.Internal,
                null,
                environment.IsDevelopment() ? exception.StackTrace : null);
        }

        private static object? LocalizeValidationErrors(object? errors,JsonStringLocalizer localizer,string culture)
        {
            if (errors is not IReadOnlyDictionary<string, LocalizationKeys.LocalizationError[]> keyedErrors)
            {
                return errors;
            }

            return keyedErrors.ToDictionary(
                entry => entry.Key,
                entry => entry.Value
                    .Select(error => new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["field"] = error.MessageArgs?.TryGetValue("field", out var field) == true
                            ? field
                            : entry.Key,
                        ["localizationKey"] = error.LocalizationKey,
                        ["message"] = localizer.GetString(error.LocalizationKey, culture, error.MessageArgs)
                    })
                    .ToArray(),
                StringComparer.OrdinalIgnoreCase);
        }

        private sealed record MappedException(
            int StatusCode,
            string ErrorCode,
            string Title,
            string Detail,
            string LocalizationKey,
            object? MessageArgs,
            object? Errors);
    }



}
