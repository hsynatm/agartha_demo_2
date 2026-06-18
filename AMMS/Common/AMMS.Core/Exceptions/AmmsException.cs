using AMMS.Core.Localization;

namespace AMMS.Core.Exceptions
{
    public abstract class AmmsException : Exception
    {
        public string ErrorCode { get; }
        public string LocalizationKey { get; }
        public object? MessageArgs { get; }
        public object? Details { get; }


        protected AmmsException(string errorCode, string localizationKey,object? messageArgs = null,object? details = null, Exception? innerException = null)  : base(localizationKey, innerException)
        {
            ErrorCode = errorCode;
            LocalizationKey = localizationKey;
            MessageArgs = messageArgs;
            Details = details;
        }      

        public sealed class Business : AmmsException
        {
            public Business(
                string localizationKey,
                string errorCode = "BUSINESS_ERROR",
                object? messageArgs = null,
                object? details = null,
                Exception? innerException = null)
                : base(errorCode, localizationKey, messageArgs, details, innerException)
            {
            }
        }

        public sealed class Validation : AmmsException
        {
            public Validation(IReadOnlyDictionary<string, LocalizationKeys.LocalizationError[]> errors)
                : base("VALIDATION_ERROR", LocalizationKeys.Shared.ValidationSummary, details: errors)
            {
            }
        }

        public sealed class NotFound : AmmsException
        {
            public NotFound(
                string localizationKey,
                object? messageArgs = null,
                object? details = null,
                string errorCode = "NOT_FOUND")
                : base(errorCode, localizationKey, messageArgs, details)
            {
            }

            public static NotFound ForEntity(string localizationKey, string errorCode, Guid entityId) =>
                new(
                    localizationKey,
                    messageArgs: new { EntityId = entityId },
                    details: new { EntityId = entityId },
                    errorCode: errorCode);
        }

        public sealed class Unauthorized : AmmsException
        {
            public Unauthorized(
                string localizationKey = LocalizationKeys.Shared.Unauthorized,
                object? messageArgs = null,
                object? details = null,
                string errorCode = "UNAUTHORIZED")
                : base(errorCode, localizationKey, messageArgs, details)
            {
            }
        }

        public sealed class Forbidden : AmmsException
        {
            public Forbidden(
                string localizationKey = LocalizationKeys.Shared.Forbidden,
                object? messageArgs = null,
                object? details = null,
                string errorCode = "FORBIDDEN")
                : base(errorCode, localizationKey, messageArgs, details)
            {
            }
        }
    }
}
