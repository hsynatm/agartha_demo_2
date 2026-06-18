using AMMS.Core.Exceptions;
using AMMS.Core.Localization;
using Microsoft.AspNetCore.Mvc;

namespace AMMS.Api.Controllers
{

    [ApiController]
    public abstract class ApiBaseController : ControllerBase
    {
        protected void EnsureValidRequest()
        {
            if (ModelState.IsValid)
            {
                return;
            }

            var errors = ModelState
                .Where(entry => entry.Value?.Errors.Count > 0)
                .ToDictionary(
                    entry => entry.Key,
                    entry => entry.Value!.Errors
                        .Select(error => CreateValidationError(entry.Key, error.ErrorMessage))
                        .ToArray());

            throw new AmmsException.Validation(errors);
        }

        private static LocalizationKeys.LocalizationError CreateValidationError(string fieldName, string? errorMessage)
        {
            var localizationKey = LocalizationKeys.IsLocalizationKey(errorMessage)
                ? errorMessage!
                : LocalizationKeys.Shared.Invalid;

            return new LocalizationKeys.LocalizationError(
                localizationKey,
                new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                {
                    ["field"] = fieldName
                });
        }
    }

}
