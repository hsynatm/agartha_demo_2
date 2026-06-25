using AMMS.Core.Exceptions;
using AMMS.Core.Localization;
using AMMS.Infrastructure.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AMMS.Api.Controllers
{

    [ApiController]
    [Authorize]
    public abstract class ApiBaseController : ControllerBase
    {
        protected void EnsureValidRequest(object? model = null)
        {
            if (model is null)
            {
                ModelState.AddModelError(nameof(model), LocalizationKeys.Shared.Required);
            }
            else
            {
                NonNullableModelValidator.Validate(model, ModelState);
            }

            if (ModelState.IsValid)
            {
                return;
            }

            var errors = ModelState
                .Where(entry => entry.Value?.Errors.Count > 0)
                .SelectMany(entry => entry.Value!.Errors.Select(error => (Field: NormalizeFieldName(entry.Key), Error: error)))
                .GroupBy(item => item.Field, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .Select(item => CreateValidationError(group.Key, item.Error.ErrorMessage))
                        .ToArray(),
                    StringComparer.OrdinalIgnoreCase);

            throw new AmmsException.Validation(errors);
        }

        private static LocalizationKeys.LocalizationError CreateValidationError(string fieldName, string? errorMessage)
        {
            return new LocalizationKeys.LocalizationError(
                ResolveLocalizationKey(errorMessage),
                new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                {
                    ["field"] = fieldName
                });
        }

        private static string ResolveLocalizationKey(string? errorMessage)
        {
            if (LocalizationKeys.IsLocalizationKey(errorMessage))
            {
                return errorMessage!;
            }

            if (string.IsNullOrWhiteSpace(errorMessage))
            {
                return LocalizationKeys.Shared.Invalid;
            }

            if (errorMessage.Contains("required", StringComparison.OrdinalIgnoreCase))
            {
                return LocalizationKeys.Shared.Required;
            }

            if (errorMessage.Contains("maximum", StringComparison.OrdinalIgnoreCase)
                || errorMessage.Contains("max length", StringComparison.OrdinalIgnoreCase)
                || errorMessage.Contains("character", StringComparison.OrdinalIgnoreCase))
            {
                return LocalizationKeys.Shared.MaxLength;
            }

            return LocalizationKeys.Shared.Invalid;
        }

        private static string NormalizeFieldName(string modelStateKey)
        {
            if (string.IsNullOrWhiteSpace(modelStateKey))
            {
                return modelStateKey;
            }

            if (modelStateKey.StartsWith("$.", StringComparison.Ordinal))
            {
                return ToCamelCase(modelStateKey[2..]);
            }

            if (string.Equals(modelStateKey, "$", StringComparison.Ordinal))
            {
                return modelStateKey;
            }

            var fieldName = modelStateKey.Contains('.')
                ? modelStateKey.Split('.')[^1]
                : modelStateKey;

            return ToCamelCase(fieldName);
        }

        private static string ToCamelCase(string value)
        {
            if (string.IsNullOrEmpty(value) || char.IsLower(value[0]))
            {
                return value;
            }

            return char.ToLowerInvariant(value[0]) + value[1..];
        }
    }

}
