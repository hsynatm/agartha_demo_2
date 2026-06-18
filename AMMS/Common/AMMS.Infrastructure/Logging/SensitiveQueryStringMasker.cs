using Microsoft.AspNetCore.WebUtilities;

namespace AMMS.Infrastructure.Logging
{


    public static class SensitiveQueryStringMasker
    {
        private const string MaskValue = "***";

        private static readonly HashSet<string> SensitiveKeys = new(StringComparer.OrdinalIgnoreCase)
        {
            "token",
            "access_token",
            "refresh_token",
            "password",
            "apikey",
            "secret",
            "authorization"
        };

        public static string? Mask(string? queryString)
        {
            if (string.IsNullOrWhiteSpace(queryString))
            {
                return null;
            }

            var normalized = queryString.TrimStart('?');
            if (normalized.Length == 0)
            {
                return null;
            }

            var parsed = QueryHelpers.ParseQuery(normalized);
            if (parsed.Count == 0)
            {
                return null;
            }

            var segments = new List<string>(parsed.Count);
            foreach (var pair in parsed)
            {
                var value = SensitiveKeys.Contains(pair.Key)
                    ? MaskValue
                    : pair.Value.ToString();

                segments.Add($"{Uri.EscapeDataString(pair.Key)}={Uri.EscapeDataString(value)}");
            }

            return string.Join('&', segments);
        }
    }





}
