namespace AMMS.Core.Localization
{
    public static class LocalizationKeys
    {
        public sealed record LocalizationError(string LocalizationKey, IReadOnlyDictionary<string, object>? MessageArgs = null);

        public static class Shared
        {
            public const string ValidationSummary = "errors.shared.validation_summary";
            public const string NotFound = "errors.shared.not_found";
            public const string Unauthorized = "errors.shared.unauthorized";
            public const string Forbidden = "errors.shared.forbidden";
            public const string Internal = "errors.shared.internal";
            public const string BadRequest = "errors.shared.bad_request";
            public const string HttpError = "errors.shared.http_error";

            public const string TitleValidation = "errors.shared.title.validation";
            public const string TitleBusiness = "errors.shared.title.business";
            public const string TitleNotFound = "errors.shared.title.not_found";
            public const string TitleUnauthorized = "errors.shared.title.unauthorized";
            public const string TitleForbidden = "errors.shared.title.forbidden";
            public const string TitleInternal = "errors.shared.title.internal";
            public const string TitleBadRequest = "errors.shared.title.bad_request";
            public const string TitleHttpError = "errors.shared.title.http_error";

            public const string Required = "validation.required";
            public const string MaxLength = "validation.max_length";
            public const string Invalid = "validation.invalid";
        }

        public static class Modules
        {
            public static class FaultManagement
            {
                public const string Folder = "fault-management";

                public const string NotFound = "errors.ariza.not_found";
                public const string CreateClosed = "errors.ariza.create_closed";
                public const string UpdateClosed = "errors.ariza.update_closed";

                public static class ErrorCodes
                {
                    public const string NotFound = "FAULT_NOT_FOUND";
                    public const string CreateClosed = "FAULT_CREATE_CLOSED";
                    public const string UpdateClosed = "FAULT_UPDATE_CLOSED";
                }
            }

            public static class MaintenanceManagement
            {
                public const string Folder = "maintenance-management";

                public const string NotFound = "errors.bakim.not_found";
                public const string UpdateFinal = "errors.bakim.update_final";
                public const string PastDate = "errors.bakim.past_date";

                public static class ErrorCodes
                {
                    public const string NotFound = "MAINTENANCE_NOT_FOUND";
                    public const string UpdateFinal = "MAINTENANCE_UPDATE_FINAL";
                    public const string PastDate = "MAINTENANCE_PAST_DATE";
                }
            }

            public static class AssetManagement
            {
                public const string Folder = "asset-management";

                public const string NotFound = "errors.varlik.not_found";

                public static class ErrorCodes
                {
                    public const string NotFound = "ASSET_NOT_FOUND";
                }
            }
        }

        public static bool IsLocalizationKey(string? value) =>
            !string.IsNullOrWhiteSpace(value)
            && (value.StartsWith("errors.", StringComparison.Ordinal)
                || value.StartsWith("validation.", StringComparison.Ordinal));
    }

}
