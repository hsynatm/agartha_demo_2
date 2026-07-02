using AMMS.Core.Exceptions;
using AMMS.Core.Localization;
using AMMS.Infrastructure.Validation;
using AMMS.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AMMS.Api.Controllers
{

    [ApiController]
    [Authorize]
    public abstract class ApiBaseController : ControllerBase
    {
        private static readonly string[] RequiredMessageKeywords = ["required"];
        private static readonly string[] MaxLengthMessageKeywords = ["maximum", "max length", "character"];

        protected async Task<ActionResult<PagedResult<T>>> OkPagedAsync<T>(
            Func<CancellationToken, Task<PagedResult<T>>> action,
            CancellationToken cancellationToken) =>
            Ok(await action(cancellationToken));

        protected async Task<ActionResult<T>> OkByIdAsync<T>(
            Func<Guid, CancellationToken, Task<T>> action,
            Guid id,
            CancellationToken cancellationToken) =>
            Ok(await action(id, cancellationToken));

        protected async Task<ActionResult<TResult>> CreatedAtGetByIdAsync<TRequest, TResult>(
            TRequest request,
            Func<TRequest, CancellationToken, Task<TResult>> action,
            Func<TResult, Guid?> idSelector,
            string getByIdActionName,
            CancellationToken cancellationToken)
        {
            EnsureValidRequest(request);
            var result = await action(request, cancellationToken);
            return CreatedAtAction(getByIdActionName, new { id = idSelector(result) }, result);
        }

        protected async Task<ActionResult<TResult>> OkValidatedAsync<TRequest, TResult>(
            TRequest request,
            Func<Guid, TRequest, CancellationToken, Task<TResult>> action,
            Guid id,
            CancellationToken cancellationToken)
        {
            EnsureValidRequest(request);
            return Ok(await action(id, request, cancellationToken));
        }

        protected async Task<IActionResult> NoContentDeleteAsync(
            Func<Guid, CancellationToken, Task> action,
            Guid id,
            CancellationToken cancellationToken)
        {
            await action(id, cancellationToken);
            return NoContent();
        }

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

            if (ContainsAnyKeyword(errorMessage, RequiredMessageKeywords))
            {
                return LocalizationKeys.Shared.Required;
            }

            if (ContainsAnyKeyword(errorMessage, MaxLengthMessageKeywords))
            {
                return LocalizationKeys.Shared.MaxLength;
            }

            return LocalizationKeys.Shared.Invalid;
        }

        private static bool ContainsAnyKeyword(string message, string[] keywords) =>
            keywords.Any(keyword => message.Contains(keyword, StringComparison.OrdinalIgnoreCase));

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
