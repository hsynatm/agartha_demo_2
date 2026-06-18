using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AMMS.Infrastructure.Localization
{
    public sealed partial class JsonStringLocalizer
    {
        private const string EmbeddedResourcePrefix = "amms.loc.";

        private static readonly Regex AcceptLanguagePartRegex = AcceptLanguagePart();

        private readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> _resourcesByCulture;
        private readonly ILogger<JsonStringLocalizer> _logger;

        public JsonStringLocalizer(ILogger<JsonStringLocalizer> logger)
        {
            _logger = logger;
            _resourcesByCulture = LoadResources();
        }

        public string GetString(string key, string? culture = null, object? messageArgs = null)
        {
            var resolvedCulture = NormalizeCulture(culture);
            var template = ResolveTemplate(key, resolvedCulture);
            return FormatTemplate(template, messageArgs);
        }

        public static string ResolveCultureFromAcceptLanguage(string? acceptLanguageHeader)
        {
            if (string.IsNullOrWhiteSpace(acceptLanguageHeader))
            {
                return CultureConstants.DefaultCulture;
            }

            foreach (var part in acceptLanguageHeader.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var match = AcceptLanguagePartRegex.Match(part);
                if (!match.Success)
                {
                    continue;
                }

                var language = match.Groups["lang"].Value.ToLowerInvariant();
                if (CultureConstants.SupportedCultures.Contains(language))
                {
                    return language;
                }
            }

            return CultureConstants.DefaultCulture;
        }

        private string ResolveTemplate(string key, string culture)
        {
            if (TryResolveTemplate(key, culture, out var template))
            {
                return template;
            }

            _logger.LogWarning(
                "Missing localization key {LocalizationKey} for culture {Culture}. Falling back to key.",
                key,
                culture);

            return key;
        }

        private bool TryResolveTemplate(string key, string culture, out string template)
        {
            if (_resourcesByCulture.TryGetValue(culture, out var cultureResources)
                && cultureResources.TryGetValue(key, out template!))
            {
                return true;
            }

            if (!string.Equals(culture, CultureConstants.DefaultCulture, StringComparison.Ordinal)
                && _resourcesByCulture.TryGetValue(CultureConstants.DefaultCulture, out var fallbackResources)
                && fallbackResources.TryGetValue(key, out template!))
            {
                return true;
            }

            template = key;
            return false;
        }

        private static string NormalizeCulture(string? culture)
        {
            if (string.IsNullOrWhiteSpace(culture))
            {
                return CultureConstants.DefaultCulture;
            }

            var normalized = culture.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)[0]
                .ToLowerInvariant();

            return CultureConstants.SupportedCultures.Contains(normalized)
                ? normalized
                : CultureConstants.DefaultCulture;
        }

        private static string FormatTemplate(string template, object? messageArgs)
        {
            if (messageArgs is null || template.IndexOf('{') < 0)
            {
                return template;
            }

            var args = ToDictionary(messageArgs);
            var formatted = template;

            foreach (var (name, value) in args)
            {
                formatted = formatted.Replace(
                    $"{{{name}}}",
                    Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty,
                    StringComparison.OrdinalIgnoreCase);
            }

            return formatted;
        }

        private static IReadOnlyDictionary<string, object> ToDictionary(object messageArgs)
        {
            if (messageArgs is IReadOnlyDictionary<string, object> readOnlyDictionary)
            {
                return readOnlyDictionary;
            }

            if (messageArgs is IDictionary<string, object> dictionary)
            {
                return dictionary as IReadOnlyDictionary<string, object>
                    ?? new Dictionary<string, object>(dictionary, StringComparer.OrdinalIgnoreCase);
            }

            var result = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            foreach (var property in messageArgs.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!property.CanRead)
                {
                    continue;
                }

                var value = property.GetValue(messageArgs);
                if (value is not null)
                {
                    result[property.Name] = value;
                }
            }

            return result;
        }

        private static IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> LoadResources()
        {
            var cultures = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
            var assembly = typeof(JsonStringLocalizer).Assembly;

            foreach (var resourceName in assembly.GetManifestResourceNames())
            {
                if (!resourceName.StartsWith(EmbeddedResourcePrefix, StringComparison.Ordinal))
                {
                    continue;
                }

                var resourcePath = resourceName[EmbeddedResourcePrefix.Length..];
                if (!resourcePath.Contains("errors.", StringComparison.Ordinal))
                {
                    continue;
                }

                var culture = resourcePath.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).LastOrDefault();
                if (string.IsNullOrWhiteSpace(culture)
                    || !CultureConstants.SupportedCultures.Contains(culture))
                {
                    continue;
                }

                if (!cultures.TryGetValue(culture, out var cultureResources))
                {
                    cultureResources = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    cultures[culture] = cultureResources;
                }

                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream is null)
                {
                    continue;
                }

                var entries = JsonSerializer.Deserialize<Dictionary<string, string>>(stream);
                if (entries is null)
                {
                    continue;
                }

                foreach (var (key, value) in entries)
                {
                    cultureResources[key] = value;
                }
            }

            return cultures.ToDictionary(
                entry => entry.Key,
                entry => (IReadOnlyDictionary<string, string>)entry.Value,
                StringComparer.OrdinalIgnoreCase);
        }

        [GeneratedRegex(@"^(?<lang>[a-zA-Z]{2,3})(?:-[a-zA-Z0-9]+)?(?:\s*;\s*q\s*=\s*(?<q>[0-9.]+))?", RegexOptions.CultureInvariant)]
        private static partial Regex AcceptLanguagePart();
    }




}
