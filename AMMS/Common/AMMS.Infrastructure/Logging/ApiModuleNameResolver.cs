using System.Globalization;

namespace AMMS.Infrastructure.Logging;

public static class ApiModuleNameResolver
{
    public static string? ResolveFromPath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length < 3)
        {
            return null;
        }

        if (!segments[0].Equals("api", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (!segments[1].StartsWith('v') || segments[1].Length < 2 || !char.IsDigit(segments[1][1]))
        {
            return null;
        }

        return ToPascalCase(segments[2]);
    }

    private static string ToPascalCase(string kebabCase)
    {
        var parts = kebabCase.Split('-', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            return kebabCase;
        }

        return string.Concat(
            parts.Select(part => char.ToUpper(part[0], CultureInfo.InvariantCulture) + part[1..]));
    }
}
